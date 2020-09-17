namespace KtTest.Models
{
    public class QuestionCategory
    {
        public Question Question { get; set; }
        public int QuestionId { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }
    }
}
