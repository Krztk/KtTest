using KtTest.Exceptions.ModelExceptions;
using System;
using System.Collections.Generic;

namespace KtTest.Models
{
    public class ChoiceAnswer : Answer
    {
        public int NumericValue { get; private set; }
        public ChoiceAnswerType ChoiceAnswerType { get; private set; }
        public bool AllValidChoicesRequired { get; private set; }
        public List<Choice> Choices { get; private set; }

        private ChoiceAnswer()
        {

        }

        public ChoiceAnswer(List<Choice> choices,
                            ChoiceAnswerType choiceAnswerType,
                            float maxScore,
                            bool allValidChoicesRequired = true)
        {
            if (choices == null)
                throw new ArgumentNullException();

            ChoiceAnswerType = choiceAnswerType;
            Choices = choices;
            NumericValue = GetNumericValueFromChoices(choices);
            MaxScore = maxScore;
            AllValidChoicesRequired = allValidChoicesRequired;
        }

        public ChoiceAnswer(int questionId,
                            List<Choice> choices,
                            ChoiceAnswerType choiceAnswerType, 
                            float maxScore,
                            bool allValidChoicesRequired = true)
                            : this(choices, choiceAnswerType, maxScore, allValidChoicesRequired)
        {
            QuestionId = questionId;
        }

        public override float GetScore(UserAnswer userAnswer)
        {
            var choiceAnswer = userAnswer as ChoiceUserAnswer;

            if (choiceAnswer == null)
                throw new WrongAnswerTypeException("Wrong answer type");

            if (choiceAnswer.QuestionId != QuestionId)
                throw new Exception("Answer.QuestionId doesn't match UserAnswer.QuestionId");

            if (NumericValue == choiceAnswer.Value)
                return MaxScore;

            if (AllValidChoicesRequired)
                return 0f;

            int userAnswerNumericValue = choiceAnswer.Value;
            int answerNumericValue = NumericValue;
            int numberOfValidChoices = 0;
            int numberOfUsersValidChoices = 0;
            while (answerNumericValue != 0)
            {
                bool isChoiceValid = (answerNumericValue & 1) == 1;
                bool isUserChoiceValid = (userAnswerNumericValue & 1) == 1;

                if (isUserChoiceValid && !isChoiceValid)
                    return 0f;

                if (isChoiceValid)
                {
                    numberOfValidChoices++;
                    if (isUserChoiceValid)
                        numberOfUsersValidChoices++;
                }

                userAnswerNumericValue >>= 1;
                answerNumericValue >>= 1;
            }

            if (userAnswerNumericValue != 0)
                return 0f;

            return MaxScore - MaxScore / numberOfValidChoices * (numberOfValidChoices - numberOfUsersValidChoices);
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
