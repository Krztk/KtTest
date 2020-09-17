using System;

namespace KtTest.Models
{
    public class WrittenAnswer : Answer
    {
        public string Value { get; set; }

        private WrittenAnswer()
        {

        }

        public WrittenAnswer(string value)
        {
            Value = value;
        }

        public override bool ValidateAnswer(UserAnswer userAnswer)
        {
            var writtenAnswer = userAnswer as WrittenUserAnswer;

            if (writtenAnswer == null || writtenAnswer.QuestionId != QuestionId)
                throw new Exception("Wrong answer type");

            return writtenAnswer.Value == Value;
        }
    }
}
