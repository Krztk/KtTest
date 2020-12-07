using KtTest.Exceptions.ModelExceptions;
using System;
using System.Collections.Generic;

namespace KtTest.Models
{
    public class ChoiceAnswer : Answer
    {
        public int NumericValue { get; private set; }
        public ChoiceAnswerType ChoiceAnswerType { get; private set; }
        public List<Choice> Choices { get; private set; }

        private ChoiceAnswer()
        {

        }

        public ChoiceAnswer(List<Choice> choices, ChoiceAnswerType choiceAnswerType)
        {
            if (choices == null)
                throw new ArgumentNullException();

            ChoiceAnswerType = choiceAnswerType;
            Choices = choices;
            NumericValue = GetNumericValueFromChoices(choices);
        }

        public override bool ValidateAnswer(UserAnswer userAnswer)
        {
            var choiceAnswer = userAnswer as ChoiceUserAnswer;

            if (choiceAnswer == null)
                throw new WrongAnswerTypeException("Wrong answer type");

            if (choiceAnswer.QuestionId != QuestionId)
                throw new Exception("Answer.QuestionId doesn't match UserAnswer.QuestionId");

            return NumericValue == choiceAnswer.Value;
        }

        private int GetNumericValueFromChoices(List<Choice> choices)
        {
            var value = 0;
            foreach (var choice in choices)
            {
                value <<= 1;
                if (choice.Valid)
                    value |= 1;
            }

            return value;
        }
    }

    public class Choice
    {
        public int Id { get; set; }
        public int ChoiceAnswerId { get; set; }
        public string Content { get; set; }
        public bool Valid { get; set; }
    }
}
