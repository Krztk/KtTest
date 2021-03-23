using System;
using System.Collections.Generic;

namespace KtTest.Models
{
    public class TestTemplate
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public ICollection<TestTemplateItem> TestItems { get; private set; } = new List<TestTemplateItem>();
        public int AuthorId { get; set; }

        private TestTemplate()
        {

        }

        public TestTemplate(int id, string name, int authorId, IEnumerable<int> questionIds)
            : this(name, authorId, questionIds)
        {
            Id = id;
        }

        public TestTemplate(string name, int authorId, IEnumerable<int> questionIds)
        {
            Name = name;
            AuthorId = authorId;

            foreach (var questionId in questionIds)
            {
                var testItem = new TestTemplateItem(questionId);
                TestItems.Add(testItem);
            }

            if (TestItems.Count == 0)
            {
                throw new ArgumentException($"{nameof(questionIds)} cannot be empty collection");
            }
        }
    }
}
