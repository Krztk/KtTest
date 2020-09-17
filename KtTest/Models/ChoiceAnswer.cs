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

            if (choiceAnswer == null || choiceAnswer.QuestionId != QuestionId)
                throw new Exception("Wrong answer type");

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
        public string Text { get; set; }
        public bool Valid { get; set; }
    }
}
