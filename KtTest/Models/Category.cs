using System.Collections.Generic;

namespace KtTest.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<QuestionCategory> QuestionCategories { get; set; }
        public int UserId { get; set; }

        private Category()
        {

        }

        public Category(string name, int userId)
        {
            Name = name;
            UserId = userId;
        }
    }
}
