using KtTest.Dtos.Test;
using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task<OperationResult<int>> CreateTest(string name, List<int> questionIds)
        {
            int authorId = userContext.UserId;
            var result = new OperationResult<int>();
            var test = new Test(name, authorId);

            foreach (var questionId in questionIds)
            {
                var testItem = new TestItem { QuestionId = questionId };
                test.TestItems.Add(testItem);
            }

            dbContext.Tests.Add(test);
            await dbContext.SaveChangesAsync();
            result.Data = test.Id;
            return result;
        }

        public async Task<PaginatedResult<Test>> GetTests(int offset, int limit)
        {
            int authorId = userContext.UserId;
            var tests = await dbContext.Tests
                .Where(x => x.AuthorId == authorId)
                .Include(x => x.TestItems)
                .Skip(offset)
                .Take(limit + 1)
                .ToListAsync();

            var result = new PaginatedResult<Test>();
            result.Data = new Paginated<Test>(limit, tests);
            return result;
        }

        public async Task<PaginatedResult<Test>> GetAvailableAndUpcomingTests(int offset, int limit)
        {
            var tests = await dbContext.UserTests
                .Include(x => x.Test)
                .Where(x => x.UserId == userContext.UserId && x.Test.EndDate > dateTimeProvider.UtcNow)
                .Skip(offset).Take(limit + 1)
                .Select(x => x.Test)
                .ToListAsync();

            var result = new PaginatedResult<Test>();
            result.Data = new Paginated<Test>(limit, tests);
            return result;
        }

        public async Task<OperationResult<Test>> GetTest(int id)
        {
            var result = new OperationResult<Test>();
            var test = await dbContext.Tests
                .Where(x => x.Id == id)
                .Include(x => x.TestItems)
                    .ThenInclude(x => x.Question)
                        .ThenInclude(x => x.Answer)
                            .ThenInclude(x => ((ChoiceAnswer)x).Choices)
            .FirstOrDefaultAsync();

            if (test == null)
            {
                result.AddFailure(Failure.NotFound());
                return result;
            }

            result.Data = test;
            return result;
        }

        public async Task<OperationResult<Test>> GetTest(int id, int authorId)
        {
            var result = new OperationResult<Test>();
            var test = await dbContext.Tests
                .Where(x => x.Id == id && x.AuthorId == authorId)
                .Include(x => x.TestItems)
                    .ThenInclude(x => x.Question)
                        .ThenInclude(x => x.Answer)
                            .ThenInclude(x => ((ChoiceAnswer)x).Choices)
            .FirstOrDefaultAsync();

            if (test == null)
            {
                result.AddFailure(Failure.NotFound());
                return result;
            }

            result.Data = test;
            return result;
        }

        public async Task<OperationResult<Test>> GetTestForStudent(int testId, int studentId)
        {
            var result = new OperationResult<Test>();
            var userTest = await dbContext.UserTests
                .Where(x => x.TestId == testId && x.UserId == studentId)
                .FirstOrDefaultAsync();

            if (userTest == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            var currentDate = dateTimeProvider.UtcNow;
            Expression<Func<Test, bool>> filterExpression;
            if (!userTest.StartDate.HasValue)
            {
                filterExpression = x => x.Id == testId && currentDate >= x.StartDate && currentDate <= x.EndDate;
            }
            else
            {
                filterExpression = x => x.Id == testId && currentDate >= userTest.StartDate && currentDate <= userTest.StartDate.Value.AddMinutes(x.Duration);
            }


            var test = await dbContext.Tests
                .Where(filterExpression)
                .Include(x => x.TestItems)
                    .ThenInclude(x => x.Question)
                        .ThenInclude(x => x.Answer)
                            .ThenInclude(x => ((ChoiceAnswer)x).Choices)
            .FirstOrDefaultAsync();

            if (test == null)
            {
                result.AddFailure(Failure.NotFound());
                return result;
            }

            if (!userTest.StartDate.HasValue)
            {
                userTest.StartDate = dateTimeProvider.UtcNow;
                await dbContext.SaveChangesAsync();
            }

            result.Data = test;
            return result;
        }

        public async Task<OperationResult<GroupResults>> GetGroupResults(int testId)
        {
            var result = new OperationResult<GroupResults>();
            var test = await dbContext.Tests.FirstOrDefaultAsync(x => x.Id == testId);
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

            var query = from userTest in dbContext.UserTests.Where(x => x.TestId == testId && x.StartDate.HasValue)
                        from user in dbContext.Users.Where(x=>x.Id == userTest.UserId)
                        from userAnswer in dbContext.UserAnswers.Where(ua => ua.TestId == userTest.TestId && ua.UserId == userTest.UserId).DefaultIfEmpty()
                        from answer in dbContext.Answers.Where(an => an.QuestionId == userAnswer.QuestionId).DefaultIfEmpty()
                        select new { userTest, user, userAnswer, answer };

            var data = await query.ToListAsync();

            //var userIdValidAnswers = new Dictionary<int, int>();
            var userIdUserTestResult = new Dictionary<int, UserTestResult>();
            foreach (var d in data)
            {
                int userId = d.user.Id;
                bool isCorrect = false;
                if (d.userAnswer == null)
                {
                    if (d.userTest.StartDate.HasValue && !CanAddAnswers(d.userTest.StartDate, test.Duration)
                        || !d.userTest.StartDate.HasValue && dateTimeProvider.UtcNow > test.EndDate.Value.AddMinutes(test.Duration))
                    {
                        UserTestResult userTestResult = new UserTestResult(d.user.UserName, 0);
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
                    userIdUserTestResult.Add(userId, new UserTestResult(d.user.UserName, isCorrect ? 1 : 0));
            }

            var groupResults = new GroupResults
            {
                Ended = testEndedResult.Data,
                NumberOfQuestion = data.Count,
                TestId = test.Id,
                TestName = test.Name,
                Results = userIdUserTestResult.Values.ToList()
            };

            result.Data = groupResults;
            return result;
        }
        
        public async Task<OperationResult> AddUserAnswers(int testId, List<UserAnswer> userAnswers)
        {
            var result = new OperationResult();
            var userTest = await dbContext.UserTests
                .Where(x => x.TestId == testId && x.UserId == userContext.UserId)
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

            var testDuration = await dbContext.Tests.Where(x => x.Id == testId).Select(x => x.Duration).FirstAsync();
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

            userTest.EndDate = dateTimeProvider.UtcNow;
            dbContext.UserAnswers.AddRange(userAnswers);
            await dbContext.SaveChangesAsync();
            return new OperationResult();
        }

        public async Task<OperationResult<List<UserAnswer>>> GetAnswers(int testId, int userId)
        {
            var result = new OperationResult<List<UserAnswer>>();
            var testTaken = await HasUserTakenTest(testId, userId);
            if (!testTaken)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            var userAnswers = await dbContext.UserAnswers
                .Where(x => x.TestId == testId && x.UserId == userId)
                .OrderBy(x => x.QuestionId)
                .ToListAsync();

            result.Data = userAnswers;
            return result;
        }

        public async Task<OperationResult> PublishTest(int testId, DateTime startDate, DateTime endDate, int durationInMinutes, IList<UserInfo> students)
        {
            var result = new OperationResult();

            if (startDate <= dateTimeProvider.UtcNow || startDate >= endDate)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            var test = await dbContext.Tests.FirstOrDefaultAsync(x => x.Id == testId);

            if (test == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            if (test.IsPublished())
            {
                result.AddFailure(Failure.BadRequest("Test is already published"));
                return result;
            }

            if (userContext.UserId != test.AuthorId)
            {
                result.AddFailure(Failure.Unauthorized());
                return result;
            }

            var numberOfQuestions = await dbContext.TestItems.Where(x => x.TestId == testId).CountAsync();
            if (numberOfQuestions == 0)
            {
                result.AddFailure(Failure.BadRequest("Cannot publish test without questions"));
                return result;
            }

            foreach (var student in students)
                test.AddUser(student.Id);


            test.Publish(startDate, endDate, dateTimeProvider.UtcNow, durationInMinutes);


            await dbContext.SaveChangesAsync();
            return result;
        }

        public bool DoesTestContainQuestions(List<int> questionIds, int testId)
        {
            var questionIdsFromDb = dbContext.TestItems
                .Where(x => x.TestId == testId)
                .Select(x => x.QuestionId)
                .ToHashSet();

            return questionIds.All(x => questionIdsFromDb.Contains(x));
        }

        public async Task<bool> HasUserTakenTest(int testId, int userId)
        {
            var hasTakenTest = await dbContext.UserAnswers
                .Where(x => x.UserId == userId && x.TestId == testId)
                .AnyAsync();

            return hasTakenTest;
        }

        public async Task<OperationResult<bool>> HasTestComeToEnd(int testId)
        {
            var result = new OperationResult<bool>();
            var test = await dbContext.Tests.Where(x => x.Id == testId).Include(x => x.UserTests).FirstOrDefaultAsync();
            if (test == null)
            {
                result.AddFailure(Failure.BadRequest($"Test with id {testId} does not exist"));
                return result;
            }

            var ended = true;
            if (dateTimeProvider.UtcNow > test.EndDate.Value.AddMinutes(test.Duration))
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

        public async Task<OperationResult<TestResults>> CheckUsersAnswers(int testId)
        {
            var result = new OperationResult<TestResults>();
            var test = await dbContext.Tests.FirstOrDefaultAsync(x => x.Id == testId);
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
                return testEndedResult.MapResult<TestResults>();
            }

            var query = from userTest in dbContext.UserTests.Where(x => x.TestId == testId && x.StartDate.HasValue)
                        from userAnswer in dbContext.UserAnswers.Where(ua => ua.TestId == userTest.TestId && ua.UserId == userTest.UserId).DefaultIfEmpty()
                        from answer in dbContext.Answers.Where(an => an.QuestionId == userAnswer.QuestionId).DefaultIfEmpty()
                        select new { userTest, userAnswer, answer };

            var data = await query.ToListAsync();

            var questionResults = new Dictionary<int, QuestionResult>();
            int numberOfUsersWhoDidntSendAnswersInTime = 0;
            foreach (var d in data)
            {
                bool isCorrect = false;
                if (d.userAnswer == null)
                {
                    if (d.userTest.StartDate.HasValue && !CanAddAnswers(d.userTest.StartDate, test.Duration))
                        numberOfUsersWhoDidntSendAnswersInTime++;

                    continue;
                }
                else
                {
                    isCorrect = d.answer.ValidateAnswer(d.userAnswer);
                }

                int questionId = d.answer.QuestionId;
                if (questionResults.ContainsKey(questionId))
                {
                    if (isCorrect)
                        questionResults[questionId].NumberOfValidAnswers++;

                    questionResults[questionId].TotalNumberOfAnswers++;
                }
                else
                    questionResults.Add(questionId, new QuestionResult(questionId, isCorrect ? 1 : 0, 1));
            }

            foreach (var questionResult in questionResults.Values)
                questionResult.TotalNumberOfAnswers += numberOfUsersWhoDidntSendAnswersInTime;

            var testResults = new TestResults(testId, testEndedResult.Data, questionResults.Values.ToList());
            result.Data = testResults;
            return result;
        }

        private bool CanAddAnswers(DateTime? startTestDate, int testDuration)
        {
            if (!startTestDate.HasValue ||
                startTestDate.Value.AddMinutes(testDuration) < dateTimeProvider.UtcNow)
            {
                return false;
            }
            return true;
        }
    }
}
