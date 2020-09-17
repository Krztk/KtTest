namespace KtTest.Models
{
    public class ChoiceUserAnswer : UserAnswer
    {
        public int Value { get; set; }

        public ChoiceUserAnswer(int value)
        {
            Value = value;
        }

        private ChoiceUserAnswer()
        {

        }
    }
}
