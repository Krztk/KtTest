using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public TestTemplate(string name, int authorId, IEnumerable<int> questionIds)
        {
            Name = name;
            AuthorId = authorId;

            foreach (var questionId in questionIds)
            {
                var testItem = new TestTemplateItem { QuestionId = questionId };
                TestItems.Add(testItem);
            }

            if (TestItems.Count == 0)
            {
                throw new ArgumentException($"{nameof(questionIds)} cannot be empty collection");
            }
        }
    }
}
