using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public class Test
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public ICollection<TestItem> TestItems { get; private set; } = new List<TestItem>();
        public ICollection<UserTest> UserTests { get; private set; } = new List<UserTest>();
        public int AuthorId { get; set; }
        public int Duration { get; set; }
        public DateTime? PublishedAt { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        private Test()
        {

        }

        public Test(string name, int authorId)
        {
            Name = name;
            AuthorId = authorId;
        }

        public void AddUser(int userId)
        {
            UserTests.Add(new UserTest(userId, Id));
        }

        public void Publish(DateTime startDate, DateTime endDate, DateTime publishDate, int durationInMinutes)
        {
            if (startDate >= endDate)
                throw new Exception();

            if (UserTests.Count == 0)
                throw new Exception("Cannot publish a test if there aren't users available");

            StartDate = startDate;
            EndDate = endDate;
            PublishedAt = publishDate;
            Duration = durationInMinutes;
        }

        public bool IsPublished()
        {
            return PublishedAt.HasValue;
        }
    }
}
