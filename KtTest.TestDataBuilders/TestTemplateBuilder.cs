using KtTest.Models;
using System.Collections.Generic;

namespace KtTest.TestDataBuilders
{
    public class TestTemplateBuilder
    {
        private int authorId;
        private IEnumerable<int> questionIds;
        private string name = "default name";

        public TestTemplateBuilder(int authorId, IEnumerable<int> questionIds)
        {
            this.authorId = authorId;
            this.questionIds = questionIds;
        }

        public TestTemplateBuilder WithName(string name)
        {
            this.name = name;
            return this;
        }

        public TestTemplate Build()
        {
            return new TestTemplate(name, authorId, questionIds);
        }
    }
}
