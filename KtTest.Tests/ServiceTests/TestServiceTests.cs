using FluentAssertions;
using KtTest.Models;
using KtTest.Services;
using Microsoft.EntityFrameworkCore;
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
            var result = await service.CreateTest(testName, questionIds);

            //assert
            result.Succeeded.Should().BeTrue();
            var persistedTest = dbContext.Tests.Where(x => x.Id == result.Data).FirstOrDefault();
            persistedTest.Should().NotBeNull();
            persistedTest.Name.Should().Be(testName);
            var testItemIds = dbContext.TestItems.Where(x => x.TestId == result.Data).OrderBy(x => x.QuestionId).Select(x => x.QuestionId).ToList();
            testItemIds.Should().BeEquivalentTo(questionIds);
        }

        [Fact]
        public async Task HasTestComeToEnd_NoUserHasStartedTest_ReturnsSuccessResultWithFalse()
        {
            //arrange
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));
            int userId = 3;
            var test = PrepareTestForUsers(dateTimeProdiver.Object, 2, userId, 4);
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
        public async Task CheckUsersAnswers_SampleData_ReturnsSuccessResult()
        {
            //arrange
            var userId = 1;
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc));

            int[] usersWhoCanTakeTest = new int[] { 2, 3, 4, 5, 6, 7, 8 };
            int[] usersWhoStartedTestButDidntSendAnswers = new int[] { 7, 8 };
            int[] usersWhoSentAnswers = new int[] { 2, 3, 4 };

            var test = SeedTestWithAnswers(userId,
                usersWhoCanTakeTest,
                usersWhoStartedTestButDidntSendAnswers,
                usersWhoSentAnswers,
                dateTimeProdiver.Object);

            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new TestService(dbContext, userContextMock.Object, dateTimeProdiver.Object);

            //act
            var result = await service.CheckUsersAnswers(test.Id);

            //assert
            result.Succeeded.Should().BeTrue();
            result.Data.TestFinished.Should().BeTrue();

            var questionIds = test.TestItems.Select(x => x.QuestionId).ToArray();
            var totalNumberOfAnswers = usersWhoStartedTestButDidntSendAnswers.Length + usersWhoSentAnswers.Length;

            result.Data.QuestionResults[0].Should().BeEquivalentTo(new QuestionResult(questionIds[0], 2, totalNumberOfAnswers));
            result.Data.QuestionResults[1].Should().BeEquivalentTo(new QuestionResult(questionIds[1], 3, totalNumberOfAnswers));
            result.Data.QuestionResults[2].Should().BeEquivalentTo(new QuestionResult(questionIds[2], 2, totalNumberOfAnswers));
        }

        private Test PrepareTestForUsers(IDateTimeProvider dateTimeProvider, params int[] userIds)
        {
            var test = new Test("test 1", 1);
            foreach (var userId in userIds)
            {
                test.AddUser(userId);
            }

            var publishDate = dateTimeProvider.UtcNow.AddMinutes(-10);
            var startDate = dateTimeProvider.UtcNow.AddMinutes(-5);
            var endDate = dateTimeProvider.UtcNow.AddMinutes(25);
            test.Publish(startDate, endDate, publishDate, 20);
            return test;
        }

        private Test PrepareEndedTest(IDateTimeProvider dateTimeProvider, int minutesAgo, int duration, params int[] userIds)
        {
            var test = new Test("test 1", 1);
            foreach (var userId in userIds)
            {
                test.AddUser(userId);
            }


            var publishDate = dateTimeProvider.UtcNow.AddMinutes(-(minutesAgo + 20));
            var startDate = dateTimeProvider.UtcNow.AddMinutes(-(minutesAgo + 10));
            var endDate = dateTimeProvider.UtcNow.AddMinutes(-minutesAgo);
            test.Publish(startDate, endDate, publishDate, duration);
            return test;
        }

        private Test SeedTestWithAnswers(int authorId,
            int[] usersWhoCanTakeTest,
            int[] usersWhoStartedTestButDidntSendAnswers,
            int[] usersWhoSentAnswers,
            IDateTimeProvider dateTimeProvider)
        {
            int numberOfMinutesAfterTestEnd = 15;
            var test = PrepareEndedTest(dateTimeProvider, numberOfMinutesAfterTestEnd, 10, usersWhoCanTakeTest);
            foreach (var userTest in test.UserTests)
            {
                if (usersWhoStartedTestButDidntSendAnswers.Contains(userTest.UserId)
                    || usersWhoSentAnswers.Contains(userTest.UserId))
                {
                    userTest.StartDate = dateTimeProvider.UtcNow.AddMinutes(-numberOfMinutesAfterTestEnd - 3);
                }
                else if (usersWhoSentAnswers.Contains(userTest.UserId))
                {
                    userTest.EndDate = dateTimeProvider.UtcNow.AddMinutes(-numberOfMinutesAfterTestEnd - 1);
                }
            }

            var questions = new List<Question>
            {
                new Question("2+2", new WrittenAnswer("4"), authorId),
                new Question("1+2*3", new WrittenAnswer("7"), authorId),
                new Question("Select numbers divisible by 4",
                    new ChoiceAnswer(
                        new List<Choice>
                        {
                            new Choice {Text = "4", Valid = true},
                            new Choice {Text = "5", Valid = false},
                            new Choice {Text = "6", Valid = false},
                            new Choice {Text = "92", Valid = true},
                        },
                        ChoiceAnswerType.MultipleChoice),
                    authorId)
            };

            foreach (var question in questions)
                test.TestItems.Add(new TestItem { Question = question });

            dbContext.Add(test);
            dbContext.SaveChanges();

            var usersAnswers = new List<UserAnswer>();
            foreach (var userId in usersWhoSentAnswers)
            {
                var answer = userId == 3 ? "5" : "4";
                UserAnswer userAnswer = new WrittenUserAnswer(answer)
                {
                    QuestionId = questions[0].Id,
                    TestId = test.Id,
                    UserId = userId
                };
                usersAnswers.Add(userAnswer);

                userAnswer = new WrittenUserAnswer("7")
                {
                    QuestionId = questions[1].Id,
                    TestId = test.Id,
                    UserId = userId
                };
                usersAnswers.Add(userAnswer);

                var numericAnswer = userId == 2 ? 8 : 9;
                userAnswer = new ChoiceUserAnswer(numericAnswer)
                {
                    QuestionId = questions[2].Id,
                    TestId = test.Id,
                    UserId = userId
                };
                usersAnswers.Add(userAnswer);
            }

            dbContext.UserAnswers.AddRange(usersAnswers);
            dbContext.SaveChanges();
            return test;
        }
    }
}
