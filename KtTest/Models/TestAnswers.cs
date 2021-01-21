using KtTest.Services;
using System;
using System.Collections.Generic;

namespace KtTest.Models
{
    public class TestAnswers
    {
        private readonly IDateTimeProvider dateTimeProvider;
        public ScheduledTest ScheduledTest { get; private set; }
        public UserTest UserTest { get; private set; }
        public string UserName { get; private set; }
        private List<AnswerPair> answerPairs  = new List<AnswerPair>();

        public TestAnswers(ScheduledTest scheduledTest, UserTest userTest, string userName, IDateTimeProvider dateTimeProvider)
        {
            UserTest = userTest;
            UserName = userName;
            this.dateTimeProvider = dateTimeProvider;
            ScheduledTest = scheduledTest;
        }

        public void AddAnswerPair(UserAnswer userAnswer, Answer answer)
        {
            answerPairs.Add(new AnswerPair(userAnswer, answer));
        }

        public UserTestResult GetTestResult()
        {
            TestStatus testStatus = GetTestStatus();
            if (testStatus == TestStatus.Completed)
            {
                float userScore = GetTestScore();
                return new UserTestResult(UserName, userScore, UserTest.UserId, testStatus);
            }

            return new UserTestResult(UserName, null, UserTest.UserId, testStatus);
        }

        private float GetTestScore()
        {
            float userScore = 0f;
            foreach (var answerPair in answerPairs)
            {
                userScore += answerPair.GetUserScore();
            }
            return userScore;
        }

        private TestStatus GetTestStatus()
        {
            if (UserTest.EndDate.HasValue)
                return TestStatus.Completed;

            if (!UserTest.StartDate.HasValue)
            {
                if (ScheduledTest.EndDate > dateTimeProvider.UtcNow)
                    return TestStatus.UserHasntStartedTestInTime;

                return TestStatus.UserHasntStartedTestYet;
            }

            if (UserTest.StartDate.HasValue)
            {
                if (UserTest.StartDate.Value.AddMinutes(ScheduledTest.Duration) > dateTimeProvider.UtcNow)
                    return TestStatus.IsInProcess;

                return TestStatus.UserHasntSentAnswersInTime;
            }

            throw new Exception("Illegal State");
        }
    }

    public class AnswerPair
    {
        public UserAnswer UserAnswer { get; private set; }
        public Answer Answer { get; private set; }

        public AnswerPair(UserAnswer userAnswer, Answer answer)
        {
            UserAnswer = userAnswer;
            Answer = answer;
        }

        public float GetUserScore()
        {
            return Answer.GetScore(UserAnswer);
        }
    }
}
