using KtTest.Models;
using System;
using System.Linq;

namespace KtTest.IntegrationTests.Helpers
{
    public class UserAnswerGenerator
    {
        public static UserAnswer GenerateValidAnswer(Question question, int scheduledTestId, int userId)
        {
            var answer = question.Answer;
            if (answer is WrittenAnswer written)
            {
                return new WrittenUserAnswer(written.Value, scheduledTestId, question.Id, userId);
            }
            else if (answer is ChoiceAnswer choiceAnswer)
            {
                return new ChoiceUserAnswer(choiceAnswer.NumericValue, scheduledTestId, question.Id, userId);
            }

            throw new Exception("Wrong answer type");
        }

        public static UserAnswer GenerateInvalidAnswer(Question question, int scheduledTestId, int userId)
        {
            var answer = question.Answer;
            if (answer is WrittenAnswer written)
            {
                return new WrittenUserAnswer(written.Value + "Invalid", scheduledTestId, question.Id, userId);
            }
            else if (answer is ChoiceAnswer choiceAnswer)
            {
                return new ChoiceUserAnswer(0, scheduledTestId, question.Id, userId);
            }

            throw new Exception("Wrong answer type");
        }

        public static UserAnswer GenerateUserAnswerWithNValidChoices(Question question, int validChoices, int scheduledTestId, int userId)
        {
            var answer = question.Answer as ChoiceAnswer;
            if (answer == null)
                throw new Exception("Wrong answer type");

            int validChoicesCount = answer.Choices.Where(x => x.Valid).Count();
            if (validChoicesCount < validChoices)
                throw new ArgumentOutOfRangeException(nameof(validChoices));

            if (validChoicesCount == validChoices)
                return new ChoiceUserAnswer(answer.NumericValue, scheduledTestId, question.Id, userId);

            int numberOfChoicesToChange = validChoicesCount - validChoices;
            int tempNumericValue = answer.NumericValue;
            while (tempNumericValue != 0 && numberOfChoicesToChange != 0)
            {
                if ((tempNumericValue & 1) == 1)
                    numberOfChoicesToChange--;

                tempNumericValue >>= 1;
            }

            return new ChoiceUserAnswer(tempNumericValue, scheduledTestId, question.Id, userId);
        }
    }
}
