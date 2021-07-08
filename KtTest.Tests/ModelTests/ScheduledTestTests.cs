using FluentAssertions;
using KtTest.Models;
using KtTest.Services;
using KtTest.TestDataBuilders;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace KtTest.Tests.ModelTests
{
    public class ScheduledTestTests
    {
        [Fact]
        public void HasTestComeToEnd_OneUserDidntSendAnswersAndStillHasTimeToDoSo_ReturnsFalse()
        {
            //arrange
            var utcNow = new DateTime(2021, 7, 8, 19, 17, 28, DateTimeKind.Utc);
            var testPublishDate = utcNow.AddHours(-2);
            var testStartDate = utcNow.AddHours(-1);
            var testEndDate = utcNow.AddHours(3);
            var scheduledTestId = 8;

            var userTests = new List<UserTest>
            {
                new UserTestBuilder(11)
                    .WithScheduledTestId(scheduledTestId)
                    .WithStartDate(testStartDate.AddMinutes(5))
                    .WithEndDate(testStartDate.AddMinutes(30))
                    .Build(),
                new UserTestBuilder(12)
                    .WithScheduledTestId(scheduledTestId)
                    .WithStartDate(testStartDate.AddMinutes(3))
                    .WithEndDate(testStartDate.AddMinutes(10))
                    .Build(),
                new UserTestBuilder(13)
                    .WithScheduledTestId(scheduledTestId)
                    .WithStartDate(testStartDate.AddMinutes(5))
                    .Build(),
            };

            var scheduledTest = new ScheduledTestBuilder(1, utcNow)
                .WithDates(testPublishDate, testStartDate, testEndDate)
                .WithDuration(120)
                .WithUserTests(userTests)
                .Build();

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);

            //act
            var result = scheduledTest.HasTestComeToEnd(dateTimeProviderMock.Object);

            //assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HasTestComeToEnd_EveryUserSentAnswers_ReturnsTrue()
        {
            //arrange
            var utcNow = new DateTime(2021, 7, 8, 19, 17, 28, DateTimeKind.Utc);
            var testPublishDate = utcNow.AddHours(-2);
            var testStartDate = utcNow.AddHours(-1);
            var testEndDate = utcNow.AddHours(3);
            var scheduledTestId = 8;

            var userTests = new List<UserTest>
            {
                new UserTestBuilder(11)
                    .WithScheduledTestId(scheduledTestId)
                    .WithStartDate(testStartDate.AddMinutes(5))
                    .WithEndDate(testStartDate.AddMinutes(30))
                    .Build(),
                new UserTestBuilder(12)
                    .WithScheduledTestId(scheduledTestId)
                    .WithStartDate(testStartDate.AddMinutes(3))
                    .WithEndDate(testStartDate.AddMinutes(10))
                    .Build(),
                new UserTestBuilder(13)
                    .WithScheduledTestId(scheduledTestId)
                    .WithStartDate(testStartDate.AddMinutes(5))
                    .WithEndDate(testStartDate.AddMinutes(10))
                    .Build(),
            };

            var scheduledTest = new ScheduledTestBuilder(1, utcNow)
                .WithDates(testPublishDate, testStartDate, testEndDate)
                .WithDuration(120)
                .WithUserTests(userTests)
                .Build();

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);

            //act
            var result = scheduledTest.HasTestComeToEnd(dateTimeProviderMock.Object);

            //assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasTestComeToEnd_NoOneSentAnswersAndTimeWindowForStartingTestDoesntExistAnymore_ReturnsTrue()
        {
            //arrange
            var utcNow = new DateTime(2021, 7, 8, 19, 17, 28, DateTimeKind.Utc);
            var testPublishDate = utcNow.AddHours(-5);
            var testStartDate = utcNow.AddHours(-3);
            var testEndDate = utcNow.AddHours(-2);
            var scheduledTestId = 8;

            var userTests = new List<UserTest>
            {
                new UserTestBuilder(11)
                    .WithScheduledTestId(scheduledTestId)
                    .Build(),
                new UserTestBuilder(12)
                    .WithScheduledTestId(scheduledTestId)
                    .Build(),
                new UserTestBuilder(13)
                    .WithScheduledTestId(scheduledTestId)
                    .Build(),
            };

            var scheduledTest = new ScheduledTestBuilder(1, utcNow)
                .WithDates(testPublishDate, testStartDate, testEndDate)
                .WithDuration(120)
                .WithUserTests(userTests)
                .Build();

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);

            //act
            var result = scheduledTest.HasTestComeToEnd(dateTimeProviderMock.Object);

            //assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasTestComeToEnd_TimeWindowForStartingTestDoesntExistAnymoreButThereIsUserWhoStartedTestInTime_ReturnsFalse()
        {
            //arrange
            var utcNow = new DateTime(2021, 7, 8, 19, 17, 28, DateTimeKind.Utc);
            var testPublishDate = utcNow.AddHours(-5);
            var testStartDate = utcNow.AddHours(-3);
            var testEndDate = utcNow.AddMinutes(-10);
            var scheduledTestId = 8;

            var userTests = new List<UserTest>
            {
                new UserTestBuilder(11)
                    .WithScheduledTestId(scheduledTestId)
                    .Build(),
                new UserTestBuilder(12)
                    .WithScheduledTestId(scheduledTestId)
                    .Build(),
                new UserTestBuilder(13)
                    .WithScheduledTestId(scheduledTestId)
                    .WithStartDate(testEndDate.AddMinutes(-5))
                    .Build(),
            };

            var scheduledTest = new ScheduledTestBuilder(1, utcNow)
                .WithDates(testPublishDate, testStartDate, testEndDate)
                .WithDuration(60)
                .WithUserTests(userTests)
                .Build();

            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.UtcNow).Returns(utcNow);

            //act
            var result = scheduledTest.HasTestComeToEnd(dateTimeProviderMock.Object);

            //assert
            result.Should().BeFalse();
        }
    }
}
