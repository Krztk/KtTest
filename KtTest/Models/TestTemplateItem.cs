using System;

namespace KtTest.Models
{
    public class TestTemplateItem
    {
        public Question Question { get; private set; }
        public int QuestionId { get; private set; }
        public TestTemplate TestTemplate { get; private set; }
        public int TestTemplateId { get; private set; }

        private TestTemplateItem()
        {

        }
        
        public TestTemplateItem(int questionId)
        {
            QuestionId = questionId;
        }

        public class TestTemplateItemBuilder
        {
            private Question question;
            private TestTemplate testTemplate;
            private int testTemplateId = 0;
            private int? questionId;

            public TestTemplateItemBuilder()
            {

            }

            public TestTemplateItemBuilder WithQuestion(int questionId)
            {
                this.questionId = questionId;
                return this;
            }

            public TestTemplateItemBuilder WithQuestion(Question question)
            {
                this.question = question;
                questionId = question.Id;
                return this;
            }

            public TestTemplateItemBuilder WithTestTemplate(int testTemplateId)
            {
                this.testTemplateId = testTemplateId;
                return this;
            }

            public TestTemplateItemBuilder WithTestTemplate(TestTemplate testTemplate)
            {
                this.testTemplate = testTemplate;
                testTemplateId = testTemplate.Id;
                return this;
            }

            public TestTemplateItem Build()
            {
                if (!questionId.HasValue)
                {
                    throw new Exception("Cannot build TestTemplateItem without question or questionId");
                }
                var testTemplateItem = new TestTemplateItem(questionId.Value);
                testTemplateItem.Question = question;
                testTemplateItem.TestTemplate = testTemplate;
                testTemplateItem.TestTemplateId = testTemplateId;

                return testTemplateItem;
            }
        }
    }
}
