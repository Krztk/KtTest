using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KtTest.Models
{
    public class ScheduledTest
    {
        public int Id { get; private set; }
        public int TestTemplateId { get; private set; }
        public TestTemplate TestTemplate { get; private set; }
        public int Duration { get; private set; }
        public DateTime PublishedAt { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        private readonly List<UserTest> userTests = new List<UserTest>();
        public IReadOnlyCollection<UserTest> UserTests => userTests.AsReadOnly();

        public ScheduledTest(int testTemplateId,
            DateTime publishedAt,
            DateTime startDate,
            DateTime endDate,
            int duration,
            IEnumerable<int> userIds,
            int scheduledTestId = 0) : this(scheduledTestId, testTemplateId, publishedAt, startDate, endDate, duration)
        {
            foreach (var userId in userIds)
            {
                userTests.Add(new UserTest(userId, Id));
            }

            if (userTests.Count == 0)
                throw new ArgumentException($"{nameof(userIds)} cannot be empty collection");
        }

        public ScheduledTest(
            int testTemplateId,
            DateTime publishedAt,
            DateTime startDate,
            DateTime endDate,
            int duration,
            IEnumerable<UserTest> userTests,
            int scheduledTestId) : this(scheduledTestId, testTemplateId, publishedAt, startDate, endDate, duration)
        {
            Id = scheduledTestId;
            this.userTests = userTests.ToList();
        }

        private ScheduledTest(
            int scheduledTestId,
            int testTemplateId,
            DateTime publishedAt,
            DateTime startDate,
            DateTime endDate,
            int duration)
        {
            if (startDate >= endDate) throw new Exception("StartDate cannot be equal or higher than EndDate");

            Id = scheduledTestId;
            TestTemplateId = testTemplateId;
            PublishedAt = publishedAt;
            StartDate = startDate;
            EndDate = endDate;
            Duration = duration;
        }
        private ScheduledTest()
        {
            //ef
        }

        public bool HasTestComeToEnd(IDateTimeProvider dateTimeProvider)
        {
            if (dateTimeProvider.UtcNow > EndDate.AddMinutes(Duration))
            {
                return true;
            }

            var ended = true;
            foreach (var userTest in userTests)
            {
                if (!userTest.StartDate.HasValue && dateTimeProvider.UtcNow < EndDate)
                {
                    ended = false;
                    break;
                }

                if (userTest.StartDate.HasValue
                    && !userTest.EndDate.HasValue
                    && dateTimeProvider.UtcNow < userTest.StartDate.Value.AddMinutes(Duration))
                {
                    ended = false;
                    break;
                }
            }

            return ended;
        }

        public class ScheduledTestBuilder
        {
            private int id = 0;
            private int testTemplateId = 0;
            private readonly int durationInMinutes;
            private TestTemplate testTemplate;
            private readonly DateTime publishedAt, startDate, endDate;
            private IEnumerable<int> userIds;

            public ScheduledTestBuilder(int durationInMinutes,
                DateTime publishedAt,
                DateTime startDate,
                DateTime endDate,
                IEnumerable<int> userIds)
            {
                this.durationInMinutes = durationInMinutes;
                this.publishedAt = publishedAt;
                this.startDate = startDate;
                this.endDate = endDate;
                this.userIds = userIds;
            }

            public ScheduledTestBuilder WithTestTemplate(int testTemplateId)
            {
                this.testTemplateId = testTemplateId;
                return this;
            }

            public ScheduledTestBuilder WithTestTemplate(TestTemplate testTemplate)
            {
                this.testTemplate = testTemplate;
                return this;
            }

            public ScheduledTestBuilder WithId(int id)
            {
                this.id = id;
                return this;
            }

            public ScheduledTest Build()
            {
                if (testTemplate != null)
                    testTemplateId = testTemplate.Id;

                var scheduledTest =
                    new ScheduledTest(testTemplateId, publishedAt, startDate, endDate, durationInMinutes, userIds);
                scheduledTest.Id = id;
                scheduledTest.TestTemplate = testTemplate;

                return scheduledTest;
            }
        }
    }

}
