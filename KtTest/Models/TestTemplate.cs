using System;
using System.Collections.Generic;

namespace KtTest.Models
{
    public class TestTemplate
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        private readonly List<TestTemplateItem> testItems = new List<TestTemplateItem>();
        public IReadOnlyCollection<TestTemplateItem> TestItems => testItems.AsReadOnly();
        public int AuthorId { get; set; }

        private TestTemplate()
        {
            //ef
        }

        public TestTemplate(int id, string name, int authorId, IEnumerable<int> questionIds)
            : this(name, authorId, questionIds)
        {
            Id = id;
        }

        public TestTemplate(string name, int authorId, IEnumerable<int> questionIds) : this(name, authorId)
        {
            foreach (var questionId in questionIds)
            {
                var testItem = new TestTemplateItem(questionId);
                testItems.Add(testItem);
            }

            if (testItems.Count == 0)
            {
                throw new ArgumentException($"{nameof(questionIds)} cannot be empty collection");
            }
        }

        public TestTemplate(int id, string name, int authorId, IEnumerable<Question> questions) : this(name, authorId)
        {
            Id = id;

            foreach (var question in questions)
            {
                var testItem = new TestTemplateItem(question, this);
                testItems.Add(testItem);
            }

            if (testItems.Count == 0)
            {
                throw new ArgumentException($"{nameof(questions)} cannot be empty collection");
            }
        }

        private TestTemplate(string name, int authorId)
        {
            Name = name;
            AuthorId = authorId;
        }
    }
}
