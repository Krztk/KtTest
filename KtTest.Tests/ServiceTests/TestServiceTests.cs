using FluentAssertions;
using KtTest.Models;
using KtTest.Services;
using KtTest.TestDataBuilders;
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
        private readonly int userId = 3;
        IUserContext userContext;
        public TestServiceTests()
        {
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            userContext = userContextMock.Object;
        }

        [Fact]
        public async Task CreateTest_ValidData_ReturnsSuccessResultWithId()
        {
            //arrange
            float maxScore = 3f;
            var questionsInDb = new List<Question>
            {
                new Question("1st question", new WrittenAnswer("1st question's answer", maxScore), userId),
                new Question("2st question", new WrittenAnswer("2nd question's answer", maxScore), userId),
                new Question("3st question", new WrittenAnswer("3rd question's answer", maxScore), userId),
            };
            dbContext.Questions.AddRange(questionsInDb);
            dbContext.SaveChanges();
            questionsInDb.ForEach(x => x.Id.Should().NotBe(0));
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            var service = new TestService(dbContext, userContext, dateTimeProdiver.Object);
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
        public async Task HasTestComeToEnd_NoUserHasStartedAvailableTest_ReturnsSuccessResultWithFalse()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int testAuthorId = 1;
            var questionIds = SeedQuestions(testAuthorId);
            var testTemplate = new TestTemplateBuilder(testAuthorId, questionIds).Build();
            InsertData(testTemplate);

            var scheduledTest = new ScheduledTestBuilder(testTemplate.Id, utcNow)
                .SetAsCurrentlyAvailable()
                .IncludeUser(userId)
                .Build();
            InsertData(scheduledTest);
            var service = new TestService(dbContext, userContext, dateTimeProdiver.Object);

            //act
            var result = await service.HasTestComeToEnd(scheduledTest.Id);

            //assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeFalse();
        }

        [Fact]
        public void DoesTestContainQuestions_ContainsEveryQuestionFromTest_ReturnsTrue()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int testAuthorId = 1;
            var questionIds = SeedQuestions(testAuthorId);
            var testTemplate = new TestTemplateBuilder(testAuthorId, questionIds).Build();
            InsertData(testTemplate);
            var scheduledTest = new ScheduledTestBuilder(testTemplate.Id, utcNow).Build();
            InsertData(scheduledTest);
            var service = new TestService(dbContext, userContext, dateTimeProdiver.Object);

            //act
            var doesContainEveryQuestion = service.DoesTestContainQuestions(questionIds, scheduledTest.Id);

            //assert
            doesContainEveryQuestion.Should().BeTrue();
        }

        [Fact]
        public void DoesTestContainQuestions_DoesntContainEveryQuestion_ReturnsFalse()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int testAuthorId = 1;
            var questionIds = SeedQuestions(testAuthorId);
            var notEquivalentListOfQuestionIds = new List<int>(questionIds) { 20, 21 };
            var testTemplate = new TestTemplateBuilder(testAuthorId, questionIds).Build();
            InsertData(testTemplate);
            var scheduledTest = new ScheduledTestBuilder(testTemplate.Id, utcNow).Build();
            InsertData(scheduledTest);
            var service = new TestService(dbContext, userContext, dateTimeProdiver.Object);

            //act
            var doesContainEveryQuestion = service.DoesTestContainQuestions(notEquivalentListOfQuestionIds, scheduledTest.Id);

            //assert
            doesContainEveryQuestion.Should().BeFalse();
        }

        [Fact]
        public async Task HasTestWithQuestionStarted_TestWithProvidedQuestionStarted_ReturnsTrue()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            var testTemplate = new TestTemplateBuilder(testAuthorId, questionIds).Build();
            InsertData(testTemplate);
            var scheduledTest = new ScheduledTestBuilder(testTemplate.Id, utcNow)
                .SetAsCurrentlyAvailable()
                .Build();
            InsertData(scheduledTest);
            var service = new TestService(dbContext, userContext, dateTimeProdiver.Object);

            //act
            var result = await service.HasTestWithQuestionStarted(questionIds.First());

            //assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasTestWithQuestionStarted_TestWithProvidedQuestionHasntStarted_ReturnsFalse()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            var usersTakingTest = new int[] { 2, userId, 4, 5 }.AsEnumerable();
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            var testTemplate = new TestTemplateBuilder(testAuthorId, questionIds).Build();
            InsertData(testTemplate);
            var scheduledTest = new ScheduledTestBuilder(testTemplate.Id, utcNow)
                .SetAsUpcoming()
                .Build();
            InsertData(scheduledTest);
            var service = new TestService(dbContext, userContext, dateTimeProdiver.Object);

            //act
            var result = await service.HasTestWithQuestionStarted(questionIds.First());

            //assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanGetTest_UserHasntStartedAvailableTestYet_ReturnsResultsWithTrue()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            var testTemplate = new TestTemplateBuilder(testAuthorId, questionIds).Build();
            InsertData(testTemplate);
            var scheduledTest = new ScheduledTestBuilder(testTemplate.Id, utcNow)
                .IncludeUser(userId)
                .SetAsCurrentlyAvailable()
                .Build();
            InsertData(scheduledTest);
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContext, dateTimeProdiver.Object);

            //act
            var result = await service.CanGetTest(scheduledTest.Id, userId);

            //assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public void MarkTestAsStartedIfItHasntBeenMarkedAlready_UserTestStartDateIsNull_UserTestStartDateIsSetToCurrentDate()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            var testTemplate = new TestTemplateBuilder(testAuthorId, questionIds).Build();
            InsertData(testTemplate);
            var scheduledTest = new ScheduledTestBuilder(testTemplate.Id, utcNow)
                .IncludeUser(userId)
                .SetAsCurrentlyAvailable()
                .Build();
            InsertData(scheduledTest);
            var service = new TestService(dbContext, userContext, dateTimeProdiver.Object);

            //act
            service.MarkTestAsStartedIfItHasntBeenMarkedAlready(scheduledTest.Id, userId);

            //assert
            var userTest = dbContext.UserTests.Single(x => x.ScheduledTestId == scheduledTest.Id && x.UserId == userId);
            userTest.StartDate.Should().Be(utcNow);
        }

        [Fact]
        public void MarkTestAsStartedIfItHasntBeenMarkedAlready_UserTestStartDateHasValue_UserTestStartDateHasntChanged()
        {
            //arrange
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int testAuthorId = 1;
            List<int> questionIds = SeedQuestions(testAuthorId);
            var testTemplate = new TestTemplateBuilder(testAuthorId, questionIds).Build();
            InsertData(testTemplate);
            var scheduledTest = new ScheduledTestBuilder(testTemplate.Id, utcNow)
                .IncludeUser(userId)
                .SetAsCurrentlyAvailable()
                .Build();
            InsertData(scheduledTest);
            var service = new TestService(dbContext, userContext, dateTimeProdiver.Object);
            var userTestStartDate = utcNow.AddMinutes(-5);
            dbContext.UserTests.Single(x => x.ScheduledTestId == scheduledTest.Id && x.UserId == userId).SetStartDate(userTestStartDate);

            //act
            service.MarkTestAsStartedIfItHasntBeenMarkedAlready(scheduledTest.Id, userId);

            //assert
            var userTest = dbContext.UserTests.Single(x => x.ScheduledTestId == scheduledTest.Id && x.UserId == userId);
            userTest.StartDate.Should().Be(userTestStartDate);
        }

        private List<int> SeedQuestions(int authorId)
        {
            float maxScore = 3f;
            var questions = new List<Question>
            {
                new Question("2+2", new WrittenAnswer("4", maxScore), authorId),
                new Question("1+2*3", new WrittenAnswer("7", maxScore), authorId),
                new Question("Select numbers divisible by 4",
                    new ChoiceAnswer(
                        new List<Choice>
                        {
                            new Choice {Content = "4", Valid = true},
                            new Choice {Content = "5", Valid = false},
                            new Choice {Content = "6", Valid = false},
                            new Choice {Content = "92", Valid = true},
                        },
                        ChoiceAnswerType.MultipleChoice,
                        maxScore),
                    authorId)
            };

            dbContext.Questions.AddRange(questions);
            dbContext.SaveChanges();
            return questions.Select(x => x.Id).ToList();
        }
    }
}
