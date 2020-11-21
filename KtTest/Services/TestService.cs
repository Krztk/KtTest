using KtTest.Exceptions.ServiceExcepctions;
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
            var test = await dbContext.ScheduledTests.Include(x => x.TestTemplate).FirstOrDefaultAsync(x => x.Id == testId);
            if (test == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            if (test.AuthorId != userContext.UserId)
            {
                result.AddFailure(Failure.Unauthorized());
                return result;
            }

            var testEndedResult = await HasTestComeToEnd(testId);
            if (!testEndedResult.Succeeded)
            {
                return testEndedResult.MapResult<GroupResults>();
            }

            var query = from userTest in dbContext.UserTests.Where(x => x.ScheduledTestId == testId && x.StartDate.HasValue)
                        from user in dbContext.Users.Where(x => x.Id == userTest.UserId)
                        from userAnswer in dbContext.UserAnswers.Where(ua => ua.ScheduledTestId == userTest.ScheduledTestId && ua.UserId == userTest.UserId).DefaultIfEmpty()
                        from answer in dbContext.Answers.Where(an => an.QuestionId == userAnswer.QuestionId).DefaultIfEmpty()
                        select new { userTest, user, userAnswer, answer };

            var data = await query.ToListAsync();

            var userIdUserTestResult = new Dictionary<int, UserTestResult>();
            foreach (var d in data)
            {
                int userId = d.user.Id;
                bool isCorrect = false;
                if (d.userAnswer == null)
                {
                    if (d.userTest.StartDate.HasValue && !CanAddAnswers(d.userTest.StartDate, test.Duration)
                        || !d.userTest.StartDate.HasValue && dateTimeProvider.UtcNow > test.EndDate.AddMinutes(test.Duration))
                    {
                        UserTestResult userTestResult = new UserTestResult(d.user.UserName, 0, d.user.Id);
                        userIdUserTestResult.Add(userId, userTestResult);
                    }

                    continue;
                }
                else
                {
                    isCorrect = d.answer.ValidateAnswer(d.userAnswer);
                }

                if (userIdUserTestResult.ContainsKey(userId))
                {
                    if (isCorrect)
                        userIdUserTestResult[userId].NumberOfValidAnswers++;
                }
                else
                    userIdUserTestResult.Add(userId, new UserTestResult(d.user.UserName, isCorrect ? 1 : 0, d.user.Id));
            }

            var groupResults = new GroupResults
            {
                Ended = testEndedResult.Data,
                NumberOfQuestion = data.Count,
                ScheduledTestId = test.Id,
                TestName = test.Name,
                Results = userIdUserTestResult.Values.ToList()
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

            var ended = true;
            if (dateTimeProvider.UtcNow > test.EndDate.AddMinutes(test.Duration))
            {
                result.Data = ended;
                return result;
            }

            foreach (var userTest in test.UserTests)
            {
                if (!userTest.StartDate.HasValue && dateTimeProvider.UtcNow < test.EndDate)
                {
                    ended = false;
                    break;
                }

                if (userTest.StartDate.HasValue
                    && !userTest.EndDate.HasValue
                    && dateTimeProvider.UtcNow < userTest.StartDate.Value.AddMinutes(test.Duration))
                {
                    ended = false;
                    break;
                }
            }

            result.Data = ended;
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