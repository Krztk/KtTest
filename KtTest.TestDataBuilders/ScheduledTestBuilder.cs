using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KtTest.TestDataBuilders
{
    public class ScheduledTestBuilder
    {
        private int testTemplateId;
        private readonly DateTime now;
        private DateTime publishDate;
        private DateTime startDate;
        private DateTime endDate;
        private int duration = 15;
        private List<int> userIds = new List<int> { 77, 78, 79 };

        public ScheduledTestBuilder(int testTemplateId, DateTime now)
        {
            publishDate = now.AddDays(-1);
            startDate = now.AddMinutes(-20);
            endDate = now.AddMinutes(10);
            this.testTemplateId = testTemplateId;
            this.now = now;
        }

        public ScheduledTestBuilder SetAsCurrentlyAvailable()
        {
            publishDate = now.AddDays(-1);
            startDate = now.AddMinutes(-20);
            endDate = now.AddMinutes(10);
            return this;
        }

        public ScheduledTestBuilder SetAsUpcoming()
        {
            publishDate = now.AddDays(1);
            startDate = publishDate.AddHours(8);
            endDate = startDate.AddMinutes(30);
            return this;
        }

        public ScheduledTestBuilder SetAsEnded()
        {
            publishDate = now.AddDays(-2);
            startDate = publishDate.AddHours(8);
            endDate = startDate.AddMinutes(30);
            return this;
        }

        public ScheduledTestBuilder WithDuration(int duration)
        {
            this.duration = duration;
            return this;
        }

        public ScheduledTestBuilder WithUsers(IEnumerable<int> userIds)
        {
            this.userIds = userIds.ToList();
            return this;
        }

        public ScheduledTestBuilder IncludeUser(int userId)
        {
            userIds.Add(userId);
            return this;
        }

        public ScheduledTestBuilder WithDates(DateTime publishDate, DateTime startDate, DateTime endDate)
        {
            this.publishDate = publishDate;
            this.startDate = startDate;
            this.endDate = endDate;
            return this;
        }

        public ScheduledTest Build()
        {
            return new ScheduledTest(testTemplateId, publishDate, startDate, endDate, duration, userIds);
        }
    }
}
