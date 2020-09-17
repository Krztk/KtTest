using System;

namespace KtTest.Models
{
    public class UserTest
    {
        public int UserId { get; set; }
        public Test Test { get; set; }
        public int TestId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public UserTest(int userId, int testId)
        {
            UserId = userId;
            TestId = testId;
        }

        private UserTest()
        {

        }
    }
}
