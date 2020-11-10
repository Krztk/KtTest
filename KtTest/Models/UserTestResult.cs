using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public class GroupResults
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public int NumberOfQuestion { get; set; }
        public List<UserTestResult> Results { get; set; }
        public bool Ended { get; set; }
    }

    public class UserTestResult
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int NumberOfValidAnswers { get; set; }

        public UserTestResult(string username, int validAnswers, int id)
        {
            Id = id;
            Username = username;
            NumberOfValidAnswers = validAnswers;
        }
    }
}
