namespace KtTest.Models
{
    public class ChoiceUserAnswer : UserAnswer
    {
        public int Value { get; set; }

        public ChoiceUserAnswer(int value, int scheduledTestId, int questionId, int userId)
            : base(scheduledTestId, questionId, userId)
        {
            Value = value;
        }

        private ChoiceUserAnswer() : base()
        {

        }
    }
}
