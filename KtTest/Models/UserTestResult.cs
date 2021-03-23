using System.Collections.Generic;

namespace KtTest.Models
{
    public class GroupResults
    {
        public int ScheduledTestId { get; private set; }
        public string TestName { get; private set; }
        public float MaxTestScore { get; private set; }
        public List<UserTestResult> Results { get; private set; }
        public bool Ended { get; private set; }

        public GroupResults(int scheduledTestId,
                            string testName,
                            float maxScore,
                            List<UserTestResult> results,
                            bool ended)
        {
            ScheduledTestId = scheduledTestId;
            TestName = testName;
            MaxTestScore = maxScore;
            Results = results;
            Ended = ended;
        }
    }

    public class UserTestResult
    {
        public int UserId { get; private set; }
        public string Username { get; private set; }
        public float? UserScore { get; private set; }
        public TestStatus Status { get; private set; }


        public UserTestResult(string username, float? userScore, int userId, TestStatus status)
        {
            UserId = userId;
            Username = username;
            UserScore = userScore;
            Status = status;
        }
    }

    public enum TestStatus
    {
        UserHasntSentAnswersInTime,
        UserHasntStartedTestInTime,
        UserHasntStartedTestYet,
        Completed,
        IsInProcess
    }
}