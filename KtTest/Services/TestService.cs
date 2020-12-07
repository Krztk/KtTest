using KtTest.Exceptions.ServiceExceptions;
using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
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
            var result = new OperationResult<int>();
            var test = new TestTemplate(name, authorId, questionIds);
            dbContext.TestTemplates.Add(test);
            await dbContext.SaveChangesAsync();
            result.Data = test.Id;
            return result;
        }

        public async Task<PaginatedResult<TestTemplate>> GetTestTemplates(int offset, int limit)
        {
            int authorId = userContext.UserId;
            var tests = await dbContext.TestTemplates
                .Where(x => x.AuthorId == authorId)
                .Include(x => x.TestItems)
                .Skip(offset)
                .Take(limit + 1)
                .ToListAsync();

            var result = new PaginatedResult<TestTemplate>();
            result.Data = new Paginated<TestTemplate>(limit, tests);
            return result;
        }

        public async Task<OperationResult<bool>> CanGetTest(int testId, int studentId)
        {
            var result = new OperationResult<bool>();

            var userTest = await dbContext.UserTests
                .Where(x => x.ScheduledTestId == testId && x.UserId == studentId)
                .FirstOrDefaultAsync();

            if (userTest == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            var test = await dbContext.ScheduledTests.Where(x => x.Id == testId).FirstOrDefaultAsync();
            var currentDate = dateTimeProvider.UtcNow;
            if (!userTest.StartDate.HasValue)
            {
                result.Data = currentDate >= test.StartDate && currentDate <= test.EndDate;
                return result;
            }

            result.Data = currentDate >= userTest.StartDate && currentDate <= userTest.StartDate.Value.AddMinutes(test.Duration);
            return result;
        }

        public async Task<OperationResult<GroupResults>> GetGroupResults(int testId)
        {
            var result = new OperationResult<GroupResults>();
            var scheduledTest = await dbContext.ScheduledTests.Include(x => x.TestTemplate).FirstOrDefaultAsync(x => x.Id == testId);
            if (scheduledTest == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            if (scheduledTest.TestTemplate.AuthorId != userContext.UserId)
            {
                result.AddFailure(Failure.Unauthorized());
                return result;
            }

            var testEndedResult = await HasTestComeToEnd(testId);
            if (!testEndedResult.Succeeded)
            {
                return testEndedResult.MapResult<GroupResults>();
            }

            var query = from UserTest in dbContext.UserTests.Where(x => x.ScheduledTestId == testId)
                        from User in dbContext.Users.Where(x => x.Id == UserTest.UserId)
                        from UserAnswer in dbContext.UserAnswers.Where(ua => ua.ScheduledTestId == UserTest.ScheduledTestId && ua.UserId == UserTest.UserId).DefaultIfEmpty()
                        from Answer in dbContext.Answers.Where(an => an.QuestionId == UserAnswer.QuestionId).DefaultIfEmpty()
                        select new { UserTest, UserId = User.Id, User.UserName, UserAnswer, Answer };

            int numberOfQuestions = await dbContext.TestItems.Where(x => x.TestTemplateId == scheduledTest.TestTemplateId).CountAsync();
            var queryResults = await query.ToArrayAsync();

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
            var groupResults = new GroupResults
            {
                Ended = testEndedResult.Data,
                NumberOfQuestion = numberOfQuestions,
                ScheduledTestId = scheduledTest.Id,
                TestName = scheduledTest.TestTemplate.Name,
                Results = testResults
            };

            result.Data = groupResults;
            return result;
        }

        public void MarkTestAsStartedIfItHasntBeenMarkedAlready(int testId, int userId)
        {
            var userTest = dbContext.UserTests.Local.FirstOrDefault(x => x.ScheduledTestId == testId && x.UserId == userId);
            if (userTest == null)
                throw new ValueNotInTheCacheException("the userTest should already be in cache.");

            if (userTest.StartDate.HasValue)
                return;

            userTest.SetStartDate(dateTimeProvider.UtcNow);
            dbContext.SaveChanges();
        }

        public async Task<OperationResult> AddUserAnswers(int testId, List<UserAnswer> userAnswers)
        {
            var result = new OperationResult();
            var userTest = await dbContext.UserTests
                .Where(x => x.ScheduledTestId == testId && x.UserId == userContext.UserId)
                .FirstOrDefaultAsync();

            if (userTest == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            if (!userTest.StartDate.HasValue || userTest.EndDate.HasValue)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            var testDuration = await dbContext.ScheduledTests.Where(x => x.Id == testId).Select(x => x.Duration).FirstAsync();
            if (!CanAddAnswers(userTest.StartDate, testDuration))
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            bool contains = DoesTestContainQuestions(userAnswers.Select(x => x.QuestionId).ToList(), testId);
            if (!contains)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            userTest.SetEndDate(dateTimeProvider.UtcNow);
            dbContext.UserAnswers.AddRange(userAnswers);
            await dbContext.SaveChangesAsync();
            return new OperationResult();
        }

        public async Task<OperationResult> ScheduleTest(int testId, DateTime startDate, DateTime endDate, int durationInMinutes, IList<UserInfo> students)
        {
            var result = new OperationResult();
            var utcNow = dateTimeProvider.UtcNow;

            if (startDate <= utcNow || startDate >= endDate)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            var testTemplate = await dbContext.TestTemplates.FirstOrDefaultAsync(x => x.Id == testId);

            if (testTemplate == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            if (userContext.UserId != testTemplate.AuthorId)
            {
                result.AddFailure(Failure.Unauthorized());
                return result;
            }

            var numberOfQuestions = await dbContext.TestItems.Where(x => x.TestTemplateId == testId).CountAsync();
            if (numberOfQuestions == 0)
            {
                result.AddFailure(Failure.BadRequest("Cannot publish test without questions"));
                return result;
            }

            IEnumerable<int> studentsIds = students.Select(x => x.Id);
            var test = new ScheduledTest(testTemplate.Id, utcNow, startDate, endDate, durationInMinutes, studentsIds);

            dbContext.ScheduledTests.Add(test);
            await dbContext.SaveChangesAsync();
            return result;
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
            var result = new OperationResult<bool>();
            var test = await dbContext.ScheduledTests.Where(x => x.Id == testId).Include(x => x.UserTests).FirstOrDefaultAsync();
            if (test == null)
            {
                result.AddFailure(Failure.BadRequest($"Test with id {testId} does not exist"));
                return result;
            }

            result.Data = test.HasTestComeToEnd(dateTimeProvider);
            return result;
        }

        public async Task<bool> HasTestWithQuestionStarted(int questionId)
        {
            return await dbContext.ScheduledTests
                .Include(x => x.TestTemplate)
                    .ThenInclude(x => x.TestItems)
                .Where(x => x.StartDate <= dateTimeProvider.UtcNow && x.TestTemplate.TestItems.Any(x => x.QuestionId == questionId))
                .CountAsync() > 0;
        }

        private bool CanAddAnswers(DateTime? userTestStartDate, int testDuration)
        {
            return userTestStartDate.HasValue
                && userTestStartDate.Value.AddMinutes(testDuration) > dateTimeProvider.UtcNow;
        }
    }
}