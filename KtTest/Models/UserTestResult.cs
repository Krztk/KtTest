using System.Collections.Generic;

namespace KtTest.Models
{
    public class GroupResults
    {
        public int ScheduledTestId { get; set; }
        public string TestName { get; set; }
        public int NumberOfQuestion { get; set; }
        public List<UserTestResult> Results { get; set; }
        public bool Ended { get; set; }
    }

    public class UserTestResult
    {
        public int UserId { get; private set; }
        public string Username { get; private set; }
        public int? NumberOfValidAnswers { get; set; }
        public TestStatus Status { get; private set; }


        public UserTestResult(string username, int? validAnswers, int userId, TestStatus status)
        {
            UserId = userId;
            Username = username;
            NumberOfValidAnswers = validAnswers;
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