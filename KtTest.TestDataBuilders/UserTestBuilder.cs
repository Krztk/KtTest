using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KtTest.TestDataBuilders
{
    public class UserTestBuilder
    {
        private readonly int userId;
        private int scheduledTestId;
        private DateTime? startDate;
        private DateTime? endDate;

        public UserTestBuilder(int userId)
        {
            this.userId = userId;
        }

        public UserTestBuilder WithScheduledTestId(int id)
        {
            scheduledTestId = id;
            return this;
        }

        public UserTestBuilder WithStartDate(DateTime startDate)
        {
            this.startDate = startDate;
            return this;
        }

        public UserTestBuilder WithEndDate(DateTime endDate)
        {
            this.endDate = endDate;
            return this;
        }

        public UserTest Build()
        {
            var userTest = new UserTest(userId, scheduledTestId);
            if (startDate.HasValue)
                userTest.SetStartDate(startDate.Value);

            if (endDate.HasValue)
                userTest.SetEndDate(endDate.Value);

            return userTest;
        }
    }
}
