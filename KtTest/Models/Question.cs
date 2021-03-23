using System;
using System.Collections.Generic;

namespace KtTest.Models
{
    public class Question
    {
        public int Id { get; private set; }
        public string Content { get; private set; }
        public Answer Answer { get; private set; }
        public ICollection<TestTemplateItem> TestItems { get; private set; } = new List<TestTemplateItem>();
        public ICollection<QuestionCategory> QuestionCategories { get; private set; } = new List<QuestionCategory>();
        public int AuthorId { get; private set; }

        private Question()
        {

        }

        public Question(string content, Answer answer, int authorId)
        {
            Content = content;
            Answer = answer;
            AuthorId = authorId;
        }

        public Question(int id, string content, Answer answer, int authorId) : this(content, answer, authorId)
        {
            Id = id;
        }

        public void ReplaceCategories(IEnumerable<int> categoryIds)
        {
            QuestionCategories.Clear();
            foreach (var categoryId in categoryIds)
            {
                QuestionCategories.Add(new QuestionCategory { CategoryId = categoryId });
            }
        }

        public void UpdateContent(string content)
        {
            Content = content;
        }

        public void UpdateAnswer(Answer answer)
        {
            Answer = answer;
        }
    }
}
