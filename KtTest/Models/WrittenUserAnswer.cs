namespace KtTest.Models
{
    public class WrittenUserAnswer : UserAnswer
    {
        public string Value { get; set; }

        public WrittenUserAnswer(string value, int scheduledTestId, int questionId, int userId)
            : base(scheduledTestId, questionId, userId)
        {
            Value = value;
        }

        private WrittenUserAnswer() : base()
        {

        }
    }
}
