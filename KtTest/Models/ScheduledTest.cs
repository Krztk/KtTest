using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public class ScheduledTest
    {
        public int Id { get; private set; }
        public int TestTemplateId { get; private set; }
        public TestTemplate TestTemplate { get; private set; }
        public int Duration { get; set; }
        public DateTime PublishedAt { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public ICollection<UserTest> UserTests { get; private set; } = new List<UserTest>();

        public ScheduledTest(int testTemplateId,
            DateTime publishedAt,
            DateTime startDate,
            DateTime endDate,
            int duration,
            IEnumerable<int> userIds)
        {
            TestTemplateId = testTemplateId;
            PublishedAt = publishedAt;
            StartDate = startDate;
            EndDate = endDate;
            Duration = duration;

            foreach (var userId in userIds)
            {
                UserTests.Add(new UserTest(userId, Id));
            }

            if (UserTests.Count == 0)
                throw new ArgumentException($"{nameof(userIds)} cannot be empty collection");
        }

        private ScheduledTest()
        {
        }

        public bool HasTestComeToEnd(IDateTimeProvider dateTimeProvider)
        {
            if (dateTimeProvider.UtcNow > EndDate.AddMinutes(Duration))
            {
                return true;
            }

            var ended = true;
            foreach (var userTest in UserTests)
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
    }
}
