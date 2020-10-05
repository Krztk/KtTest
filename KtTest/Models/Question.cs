using System;
using System.Collections.Generic;

namespace KtTest.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public Answer Answer { get; set; }
        public ICollection<TestItem> TestItems { get; set; } = new List<TestItem>();
        public ICollection<QuestionCategory> QuestionCategories { get; set; } = new List<QuestionCategory>();
        public int AuthorId { get; set; }

        private Question()
        {

        }

        public Question(string content, Answer answer, int authorId)
        {
            Content = content;
            Answer = answer;
            AuthorId = authorId;
        }

        public void ReplaceCategories(IEnumerable<int> categoryIds)
        {
            QuestionCategories.Clear();
            foreach (var categoryId in categoryIds)
            {
                QuestionCategories.Add(new QuestionCategory { CategoryId = categoryId });
            }
        }
    }
}
