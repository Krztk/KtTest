namespace KtTest.Models
{
    public abstract class Answer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public abstract bool ValidateAnswer(UserAnswer userAnswer);
    }
}
