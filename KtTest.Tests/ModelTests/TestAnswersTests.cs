using FluentAssertions;
using KtTest.Models;
using KtTest.Services;
using Moq;
using System;
using Xunit;

namespace KtTest.Tests.ModelTests
{
    public class TestAnswersTests
    {
        [Fact]
        public void GetTestResult_UserHasSentAnswers_ValidResult()
        {
            //arrange
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int userId = 2;
            var scheduledTest = new ScheduledTest(1, utcNow.AddDays(-2), utcNow.AddMinutes(-60), utcNow.AddMinutes(-30), 10, new int[] { userId, 3, 4 });
            var userTest = new UserTest(2, 0);
            userTest.SetStartDate(utcNow.AddMinutes(-50));
            userTest.SetEndDate(utcNow.AddMinutes(-40));
            var testAnswers = new TestAnswers(scheduledTest, userTest, "UserName", dateTimeProdiver.Object);
            float maxScore = 3f;
            int questionId = 1;
            testAnswers.AddAnswerPair(new WrittenUserAnswer("value", 0, questionId, userId), new WrittenAnswer(questionId, "value", maxScore));
            questionId = 2;
            testAnswers.AddAnswerPair(new WrittenUserAnswer("value2", 0, questionId, userId), new WrittenAnswer(questionId, "value2", maxScore));
            var expectedUserTestResult = new UserTestResult("UserName", 6f, userId, TestStatus.Completed);

            //act
            UserTestResult result = testAnswers.GetTestResult();

            //assert
            result.Should().BeEquivalentTo(expectedUserTestResult);
        }

        [Fact]
        public void GetTestResult_UserHasntSentAnswersInTime_ValidResult()
        {
            //arrange
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int userId = 2;
            var scheduledTest = new ScheduledTest(1, utcNow.AddDays(-2), utcNow.AddMinutes(-60), utcNow.AddMinutes(-30), 10, new int[] { userId, 3, 4 });
            var userTest = new UserTest(2, 0);
            userTest.SetStartDate(utcNow.AddMinutes(-50));
            var testAnswers = new TestAnswers(scheduledTest, userTest, "UserName", dateTimeProdiver.Object);
            var expectedUserTestResult = new UserTestResult("UserName", null, userId, TestStatus.UserHasntSentAnswersInTime);

            //act
            UserTestResult result = testAnswers.GetTestResult();

            //assert
            result.Should().BeEquivalentTo(expectedUserTestResult);
        }

        [Fact]
        public void GetTestResult_UserHasBeenWritingTestFor2Minutes_ValidResult()
        {
            //arrange
            var dateTimeProdiver = new Mock<IDateTimeProvider>();
            var utcNow = new DateTime(2020, 9, 5, 14, 8, 58, 0, DateTimeKind.Utc);
            dateTimeProdiver.Setup(x => x.UtcNow).Returns(utcNow);
            int userId = 2;
            var scheduledTest = new ScheduledTest(1, utcNow.AddDays(-2), utcNow.AddMinutes(-5), utcNow.AddMinutes(10), 10, new int[] { userId, 3, 4 });
            var userTest = new UserTest(2, 0);
            userTest.SetStartDate(utcNow.AddMinutes(-2));
            var testAnswers = new TestAnswers(scheduledTest, userTest, "UserName", dateTimeProdiver.Object);
            var expectedUserTestResult = new UserTestResult("UserName", null, userId, TestStatus.IsInProcess);

            //act
            UserTestResult result = testAnswers.GetTestResult();

            //assert
            result.Should().BeEquivalentTo(expectedUserTestResult);
        }
    }
}
