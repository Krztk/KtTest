using KtTest.Exceptions.ServiceExceptions;
using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
using KtTest.Results.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public class TestService
    {
        private readonly AppDbContext dbContext;
        private readonly IUserContext userContext;
        private readonly IDateTimeProvider dateTimeProvider;

        public TestService(AppDbContext dbContext, IUserContext userContext, IDateTimeProvider dateTimeProvider)
        {
            this.dbContext = dbContext;
            this.userContext = userContext;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<OperationResult<int>> CreateTestTemplate(string name, List<int> questionIds)
        {
            int authorId = userContext.UserId;
            var test = new TestTemplate(name, authorId, questionIds);
            dbContext.TestTemplates.Add(test);
            await dbContext.SaveChangesAsync();
            return test.Id;
        }

        public async Task<OperationResult<Unit>> CanGetTest(int testId, int studentId)
        {
            var userTest = await dbContext.UserTests
                .Where(x => x.ScheduledTestId == testId && x.UserId == studentId)
                .FirstOrDefaultAsync();

            if (userTest == null)
            {
                return new BadRequestError();
            }

            var test = await dbContext.ScheduledTests.Where(x => x.Id == testId).FirstOrDefaultAsync();
            if (CanGetTest(test, userTest))
                return OperationResult.Ok;

            return new BadRequestError();
        }

        private bool CanGetTest(ScheduledTest test, UserTest userTest)
        {
            var currentDate = dateTimeProvider.UtcNow;
            if (!userTest.StartDate.HasValue)
            {
                return currentDate >= test.StartDate && currentDate <= test.EndDate;
            }

            return currentDate >= userTest.StartDate && currentDate <= userTest.StartDate.Value.AddMinutes(test.Duration);
        }

        public async Task<OperationResult<GroupResults>> GetGroupResults(int testId)
        {
            var scheduledTest = await dbContext.ScheduledTests.Include(x => x.TestTemplate).FirstOrDefaultAsync(x => x.Id == testId);
            if (scheduledTest == null)
            {
                return new BadRequestError();
            }

            if (scheduledTest.TestTemplate.AuthorId != userContext.UserId)
            {
                return new AuthorizationError();
            }

            var testEndedResult = await HasTestComeToEnd(testId);
            if (!testEndedResult.Succeeded)
            {
                return testEndedResult.Error;
            }

            var query = from UserTest in dbContext.UserTests.Where(x => x.ScheduledTestId == testId)
                        from User in dbContext.Users.Where(x => x.Id == UserTest.UserId)
                        from UserAnswer in dbContext.UserAnswers.Where(ua => ua.ScheduledTestId == UserTest.ScheduledTestId && ua.UserId == UserTest.UserId).DefaultIfEmpty()
                        from Answer in dbContext.Answers.Where(an => an.QuestionId == UserAnswer.QuestionId).DefaultIfEmpty()
                        select new { UserTest, UserId = User.Id, User.UserName, UserAnswer, Answer };

            var queryResults = await query.ToArrayAsync();
            var allAnswers = await dbContext.TestItems.Include(x => x.Question).ThenInclude(x => x.Answer)
                .Where(x => x.TestTemplateId == scheduledTest.TestTemplateId)
                .Select(x=>x.Question.Answer)
                .ToArrayAsync();

            float maxTestScore = allAnswers.Select(x => x.MaxScore).Aggregate((x, y) => x + y);

            var userIdTestAnswer = new Dictionary<int, TestAnswers>();
            foreach (var queryResult in queryResults)
            {
                var userId = queryResult.UserId;
                if (!userIdTestAnswer.ContainsKey(userId))
                {
                    userIdTestAnswer.Add(userId, new TestAnswers(scheduledTest, queryResult.UserTest, queryResult.UserName, dateTimeProvider));
                }

                userIdTestAnswer[userId].AddAnswerPair(queryResult.UserAnswer, queryResult.Answer);
            }

            var testResults = userIdTestAnswer.Select(x => x.Value.GetTestResult()).ToList();
            var groupResults = new GroupResults(scheduledTest.Id,
                                                scheduledTest.TestTemplate.Name,
                                                maxTestScore,
                                                testResults,
                                                testEndedResult.Data);

            return groupResults;
        }

        public OperationResult<Unit> MarkTestAsStarted(int testId, int userId)
        {
            var userTest = dbContext.UserTests.Local.FirstOrDefault(x => x.ScheduledTestId == testId && x.UserId == userId);
            if (userTest == null)
                throw new ValueNotInTheCacheException("the userTest should already be in cache.");

            if (userTest.StartDate.HasValue)
                return new BadRequestError($"Test with id {testId} is already set as started");

            userTest.SetStartDate(dateTimeProvider.UtcNow);
            dbContext.SaveChanges();
            return OperationResult.Ok;
        }

        public OperationResult<Unit> HasUserStartedTest(int testId, int userId)
        {
            var userTest = dbContext.UserTests.Local.FirstOrDefault(x => x.ScheduledTestId == testId && x.UserId == userId);
            if (userTest == null)
                throw new ValueNotInTheCacheException("the userTest should already be in cache.");

            if (userTest.StartDate.HasValue)
                return OperationResult.Ok;

            return new BadRequestError($"Test with id {testId} hasn't been started yet");
        }

        public async Task<OperationResult<Unit>> AddUserAnswers(int testId, List<UserAnswer> userAnswers)
        {
            var userTest = await dbContext.UserTests
                .Where(x => x.ScheduledTestId == testId && x.UserId == userContext.UserId)
                .FirstOrDefaultAsync();

            if (userTest == null)
            {
                return new BadRequestError();
            }

            if (!userTest.StartDate.HasValue || userTest.EndDate.HasValue)
            {
                return new BadRequestError();
            }

            var testDuration = await dbContext.ScheduledTests.Where(x => x.Id == testId).Select(x => x.Duration).FirstAsync();
            if (!CanAddAnswers(userTest.StartDate, testDuration))
            {
                return new BadRequestError();
            }

            bool contains = DoesTestContainQuestions(userAnswers.Select(x => x.QuestionId).ToList(), testId);
            if (!contains)
            {
                return new BadRequestError();
            }

            userTest.SetEndDate(dateTimeProvider.UtcNow);
            dbContext.UserAnswers.AddRange(userAnswers);
            await dbContext.SaveChangesAsync();
            return OperationResult.Ok;
        }

        public async Task<OperationResult<Unit>> ScheduleTest(int testId, DateTime startDate, DateTime endDate, int durationInMinutes, IList<UserInfo> students)
        {
            var utcNow = dateTimeProvider.UtcNow;

            if (startDate <= utcNow || startDate >= endDate)
            {
                return new BadRequestError();
            }

            var testTemplate = await dbContext.TestTemplates.FirstOrDefaultAsync(x => x.Id == testId);

            if (testTemplate == null)
            {
                return new BadRequestError();
            }

            if (userContext.UserId != testTemplate.AuthorId)
            {
                return new AuthorizationError();
            }

            var numberOfQuestions = await dbContext.TestItems.Where(x => x.TestTemplateId == testId).CountAsync();
            if (numberOfQuestions == 0)
            {
                return new BadRequestError("Cannot publish test without questions");
            }

            IEnumerable<int> studentsIds = students.Select(x => x.Id);
            var test = new ScheduledTest(testTemplate.Id, utcNow, startDate, endDate, durationInMinutes, studentsIds);

            dbContext.ScheduledTests.Add(test);
            await dbContext.SaveChangesAsync();
            return OperationResult.Ok;
        }

        public bool DoesTestContainQuestions(IEnumerable<int> questionIds, int testId)
        {
            var idsOfQuestionsFromDb = dbContext.TestItems
                .Join(dbContext.ScheduledTests, x => x.TestTemplateId, y => y.TestTemplateId, (x, y) => new { x.QuestionId, y.Id })
                .Where(x => x.Id == testId)
                .Select(x => x.QuestionId)
                .ToHashSet();

            return questionIds.All(x => idsOfQuestionsFromDb.Contains(x));
        }

        public async Task<bool> HasUserTakenTest(int testId, int userId)
        {
            var hasTakenTest = await dbContext.UserAnswers
                .Where(x => x.UserId == userId && x.ScheduledTestId == testId)
                .AnyAsync();

            return hasTakenTest;
        }

        public async Task<OperationResult<bool>> HasTestComeToEnd(int testId)
        {
            var test = await dbContext.ScheduledTests.Where(x => x.Id == testId).Include(x => x.UserTests).FirstOrDefaultAsync();
            if (test == null)
            {
                return new BadRequestError($"Test with id {testId} does not exist");
            }

            return test.HasTestComeToEnd(dateTimeProvider);
        }

        public async Task<bool> HasTestWithQuestionStarted(int questionId)
        {
            return await dbContext.ScheduledTests
                .Include(x => x.TestTemplate)
                    .ThenInclude(x => x.TestItems)
                .Where(x => x.StartDate <= dateTimeProvider.UtcNow && x.TestTemplate.TestItems.Any(x => x.QuestionId == questionId))
                .CountAsync() > 0;
        }

        public async Task<bool> IsQuestionIncludedInTest(int questionId)
        {
            return await dbContext.TestItems.Where(x => x.QuestionId == questionId).CountAsync() > 0;
        }

        private bool CanAddAnswers(DateTime? userTestStartDate, int testDuration)
        {
            return userTestStartDate.HasValue
                && userTestStartDate.Value.AddMinutes(testDuration) > dateTimeProvider.UtcNow;
        }
    }
}