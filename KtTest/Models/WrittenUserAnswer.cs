namespace KtTest.Models
{
    public class WrittenUserAnswer : UserAnswer
    {
        public string Value { get; set; }

        public WrittenUserAnswer(string value)
        {
            Value = value;
        }

        private WrittenUserAnswer()
        {

        }
    }
}
