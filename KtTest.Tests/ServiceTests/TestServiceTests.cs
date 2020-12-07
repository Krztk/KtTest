using FluentAssertions;
using KtTest.Models;
using KtTest.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.Tests.ServiceTests
{
    public class TestServiceTests : TestWithSqlite
    {
        [Fact]
        public async Task CreateTest_ValidData_ReturnsSuccessResultWithId()
        {
            //arrange
            var userId = 11;
            var questionsInDb = new List<Question>
            {
                new Question("1st question", new WrittenAnswer("1st question's answer"), userId),
                new Question("2st question", new WrittenAnswer("2nd question's answer"), userId),
                new Question("3st question", new WrittenAnswer("3rd question's answer"), userId),
            };
            dbContext.Questions.AddRange(questionsInDb);
            dbContext.SaveChanges();
            questionsInDb.ForEach(x => x.Id.Should().NotBe(0));
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);
            var testName = "My first test";
            var questionIds = questionsInDb.Take(2).Select(x => x.Id).ToList();

            //act
            var result = await service.CreateTestTemplate(testName, questionIds);

            //assert
            result.Succeeded.Should().BeTrue();
            var persistedTest = dbContext.TestTemplates.Where(x => x.Id == result.Data).FirstOrDefault();
            persistedTest.Should().NotBeNull();
            persistedTest.Name.Should().Be(testName);
            var testItemIds = dbContext.TestItems.Where(x => x.TestTemplateId == result.Data).OrderBy(x => x.QuestionId).Select(x => x.QuestionId).ToList();
            testItemIds.Should().BeEquivalentTo(questionIds);
        }

        [Fact]
        public async Task HasTestComeToEnd_NoUserHasStartedTest_ReturnsSuccessResultWithFalse()
        {
            //arrange
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            int userId = 3;
            int testAuthorId = 1;
            var usersTakingTest = new int[] { 2, userId, 4, 5 }.AsEnumerable();
            ScheduledTest test = PrepareTestForUsers(dateTimeProdiver.Object, testAuthorId, usersTakingTest, SeedQuestions(testAuthorId));
            dbContext.Add(test);
            dbContext.SaveChanges();
            var testId = test.Id;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);

            //act
            var result = await service.HasTestComeToEnd(testId);

            //assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeFalse();
        }

        [Fact]
        public void DoesTestContainQuestions_ContainsEveryQuestionFromTest_ReturnsTrue()
        {
            //arrange
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            int userId = 3;
            var usersTakingTest = new int[] { 2, userId, 4, 5 }.AsEnumerable();
            int testAuthorId = 1;
            var questionIds = SeedQuestions(testAuthorId);
            ScheduledTest test = PrepareTestForUsers(dateTimeProdiver.Object, testAuthorId, usersTakingTest, questionIds);
            dbContext.Add(test);
            dbContext.SaveChanges();
            var testId = test.Id;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);
            //act
            var doesContainEveryQuestion = service.DoesTestContainQuestions(questionIds, test.Id);

            //assert
            doesContainEveryQuestion.Should().BeTrue();
        }

        [Fact]
        public void DoesTestContainQuestions_DoesntContainEveryQuestion_ReturnsFalse()
        {
            //arrange
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            int userId = 3;
            var usersTakingTest = new int[] { 2, userId, 4, 5 }.AsEnumerable();
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            var notValidQuestionIdsCollection = new List<int>(questionIds) { 20, 21 };
            ScheduledTest test = PrepareTestForUsers(dateTimeProdiver.Object, testAuthorId, usersTakingTest, questionIds);
            dbContext.Add(test);
            dbContext.SaveChanges();
            var testId = test.Id;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);

            //act
            var doesContainEveryQuestion = service.DoesTestContainQuestions(notValidQuestionIdsCollection, test.Id);

            //assert
            doesContainEveryQuestion.Should().BeFalse();
        }

        [Fact]
        public async Task HasTestWithQuestionStarted_TestWithProvidedQuestionStarted_ReturnsTrue()
        {
            //arrange
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            int userId = 3;
            var usersTakingTest = new int[] { 2, userId, 4, 5 }.AsEnumerable();
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            ScheduledTest test = PrepareTestForUsers(dateTimeProdiver.Object, testAuthorId, usersTakingTest, questionIds);
            dbContext.Add(test);
            dbContext.SaveChanges();
            var testId = test.Id;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);

            //act
            var result = await service.HasTestWithQuestionStarted(questionIds.First());

            //assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasTestWithQuestionStarted_TestWithProvidedQuestionHasntStarted_ReturnsFalse()
        {
            //arrange
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            int userId = 3;
            var usersTakingTest = new int[] { 2, userId, 4, 5 }.AsEnumerable();
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            ScheduledTest test = PrepareTestForUsers(dateTimeProdiver.Object, testAuthorId, usersTakingTest, questionIds, testStarted: false);
            dbContext.Add(test);
            dbContext.SaveChanges();
            var testId = test.Id;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);

            //act
            var result = await service.HasTestWithQuestionStarted(questionIds.First());

            //assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanGetTest_UserHasntStartedTestYetAndDateRangeIsValid_ReturnsResultsWithTrue()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            DateTime publishDate = utcNow.AddDays(-1);
            DateTime startDate = utcNow.AddMinutes(-20);
            DateTime endDate = utcNow.AddMinutes(10);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            int userId = 3;
            var usersTakingTest = new int[] { 2, userId, 4, 5 }.AsEnumerable();
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            ScheduledTest test = PrepareTestForUsers("test1", publishDate, startDate, endDate, testAuthorId, 20, usersTakingTest, questionIds);
            dbContext.Add(test);
            dbContext.SaveChanges();
            var testId = test.Id;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);

            //act
            var result = await service.CanGetTest(testId, userId);

            //assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public void MarkTestAsStartedIfItHasntBeenMarkedAlready_UserTestStartDateIsNull_UserTestStartDateIsSetToCurrentDate()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            DateTime publishDate = utcNow.AddDays(-1);
            DateTime startDate = utcNow.AddMinutes(-20);
            DateTime endDate = utcNow.AddMinutes(10);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            int userId = 3;
            var usersTakingTest = new int[] { 2, userId, 4, 5 }.AsEnumerable();
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            ScheduledTest test = PrepareTestForUsers("test#1", publishDate, startDate, endDate, testAuthorId, 20, usersTakingTest, questionIds);
            dbContext.Add(test);
            dbContext.SaveChanges();
            var testId = test.Id;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);

            //act
            service.MarkTestAsStartedIfItHasntBeenMarkedAlready(testId, userId);

            //assert
            var userTest = dbContext.UserTests.Single(x => x.ScheduledTestId == testId && x.UserId == userId);
            userTest.StartDate.Should().Be(utcNow);
        }

        [Fact]
        public void MarkTestAsStartedIfItHasntBeenMarkedAlready_UserTestStartDateHasValue_UserTestStartDateHasntChanged()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            DateTime publishDate = utcNow.AddDays(-1);
            DateTime startDate = utcNow.AddMinutes(-20);
            DateTime endDate = utcNow.AddMinutes(10);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            int userId = 3;
            var usersTakingTest = new int[] { 2, userId, 4, 5 }.AsEnumerable();
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            ScheduledTest test = PrepareTestForUsers("test#1", publishDate, startDate, endDate, testAuthorId, 20, usersTakingTest, questionIds);
            dbContext.Add(test);
            dbContext.SaveChanges();
            var testId = test.Id;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);
            var userTestStartDate = utcNow.AddMinutes(-30);
            dbContext.UserTests.Single(x => x.ScheduledTestId == testId && x.UserId == userId).SetStartDate(userTestStartDate);

            //act
            service.MarkTestAsStartedIfItHasntBeenMarkedAlready(testId, userId);

            //assert
            var userTest = dbContext.UserTests.Single(x => x.ScheduledTestId == testId && x.UserId == userId);
            userTest.StartDate.Should().Be(userTestStartDate);
        }

        private ScheduledTest PrepareTestForUsers(IDateTimeProvider dateTimeProvider, int testAuthorId, IEnumerable<int> userIds, IEnumerable<int> questionsIds, bool testStarted = true)
        {
            var testTemplate = new TestTemplate("test 1", 1, questionsIds);
            dbContext.TestTemplates.Add(testTemplate);
            dbContext.SaveChanges();

            var publishDate = dateTimeProvider.UtcNow.AddMinutes(-10);
            var startDate = dateTimeProvider.UtcNow.AddMinutes(-5);
            var endDate = dateTimeProvider.UtcNow.AddMinutes(25);

            if (!testStarted)
            {
                startDate = dateTimeProvider.UtcNow.AddMinutes(5);
            }

            var scheduledTest = new ScheduledTest(testTemplate.Id, publishDate, startDate, endDate, 20, userIds);
            return scheduledTest;
        }

        private ScheduledTest PrepareTestForUsers(string testName, DateTime publishDate, DateTime startDate, DateTime endDate, int testAuthorId, int duration, IEnumerable<int> userIds, IEnumerable<int> questionsIds)
        {
            var testTemplate = new TestTemplate(testName, testAuthorId, questionsIds);
            dbContext.TestTemplates.Add(testTemplate);
            dbContext.SaveChanges();

            var scheduledTest = new ScheduledTest(testTemplate.Id, publishDate, startDate, endDate, duration, userIds);
            return scheduledTest;
        }

        private List<int> SeedQuestions(int authorId)
        {
            var questions = new List<Question>
            {
                new Question("2+2", new WrittenAnswer("4"), authorId),
                new Question("1+2*3", new WrittenAnswer("7"), authorId),
                new Question("Select numbers divisible by 4",
                    new ChoiceAnswer(
                        new List<Choice>
                        {
                            new Choice {Content = "4", Valid = true},
                            new Choice {Content = "5", Valid = false},
                            new Choice {Content = "6", Valid = false},
                            new Choice {Content = "92", Valid = true},
                        },
                        ChoiceAnswerType.MultipleChoice),
                    authorId)
            };

            dbContext.Questions.AddRange(questions);
            dbContext.SaveChanges();
            return questions.Select(x => x.Id).ToList();
        }
    }
}
