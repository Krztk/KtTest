namespace KtTest.Models
{
    public abstract class Answer
    {
        public int Id { get; protected set; }
        public int QuestionId { get; protected set; }
        public float MaxScore { get; protected set; }
        public abstract float GetScore(UserAnswer userAnswer);
    }
}
