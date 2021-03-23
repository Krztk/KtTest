using System;

namespace KtTest.Models
{
    public class UserTest
    {
        public int UserId { get; private set; }
        public ScheduledTest ScheduledTest { get; private set; }
        public int ScheduledTestId { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        public UserTest(int userId, int scheduledTestId)
        {
            UserId = userId;
            ScheduledTestId = scheduledTestId;
        }

        private UserTest()
        {

        }

        public void SetStartDate(DateTime startDate)
        {
            if (StartDate.HasValue)
                throw new Exception("Start date has been already set");

            StartDate = startDate;
        }

        public void SetEndDate(DateTime endDate)
        {
            if (EndDate.HasValue)
                throw new Exception("End date has been already set");

            EndDate = endDate;
        }
    }
}
