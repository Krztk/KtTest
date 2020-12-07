using KtTest.Exceptions.ModelExceptions;
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

            if (writtenAnswer == null)
                throw new WrongAnswerTypeException("Wrong answer type");

            if (writtenAnswer.QuestionId != QuestionId)
                throw new Exception("Answer.QuestionId doesn't match UserAnswer.QuestionId");

            return writtenAnswer.Value == Value;
        }
    }
}
