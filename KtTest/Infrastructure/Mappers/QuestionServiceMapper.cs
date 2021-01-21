using KtTest.Dtos.Test;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KtTest.Infrastructure.Mappers
{
    public class QuestionServiceMapper
    {
        public Dtos.Test.QuestionDto MapToTestQuestionDto(Question question)
        {
            if (question.Answer is WrittenAnswer writtenAnswer)
            {
                return new Dtos.Test.QuestionWithWrittenAnswerDto
                {
                    Question = question.Content,
                    Id = question.Id
                };
            }
            else if (question.Answer is ChoiceAnswer choiceAnswer)
            {
                return new Dtos.Test.QuestionWithChoiceAnswersDto
                {
                    Question = question.Content,
                    ChoiceAnswerType = choiceAnswer.ChoiceAnswerType,
                    Choices = choiceAnswer.Choices.Select(x => x.Content).ToList(),
                    Id = question.Id
                };
            }
            else
                throw new NotSupportedException();
        }

        public Dtos.Test.QuestionAnswerDto MapToTestQuestionAnswerDto(UserAnswer userAnswer)
        {
            if (userAnswer is WrittenUserAnswer written)
                return MapUserAnswer(written);
            else if (userAnswer is ChoiceUserAnswer choice)
            {
                return MapUserAnswer(choice);
            }
            else
                throw new NotSupportedException(nameof(userAnswer));
        }

        public Dtos.Test.QuestionWithResultDto MapToTestQuestionWithResultDto(Question question, UserAnswer answer)
        {
            if (question.Answer is WrittenAnswer writtenAnswer)
            {
                return new Dtos.Test.QuestionWithWrittenResultDto
                {
                    CorrectAnswer = writtenAnswer.Value,
                    UserAnswer = ((WrittenUserAnswer)answer).Value,
                    Question = question.Content,
                    QuestionId = question.Id
                };
            }
            else if (question.Answer is ChoiceAnswer choiceAnswer)
            {
                var numericValue = ((ChoiceUserAnswer)answer).Value;

                var numberOfChoices = choiceAnswer.Choices.Count;

                var choices = new List<Dtos.Test.ChoiceDto>();
                for (int i = 0; i < numberOfChoices; i++)
                {
                    bool userAnswer = (numericValue & 1) != 0;
                    numericValue >>= 1;
                    var choice = choiceAnswer.Choices[numberOfChoices - 1 - i];
                    var choiceWithAnswer = new Dtos.Test.ChoiceDto { Correct = choice.Valid, UserAnswer = userAnswer, Value = choice.Content };
                    choices.Add(choiceWithAnswer);
                }

                return new Dtos.Test.QuestionWithChoiceAnswerResultDto
                {
                    Question = question.Content,
                    QuestionId = question.Id,
                    Choices = choices,
                };
            }
            else
                throw new NotSupportedException();
        }

        private Dtos.Test.QuestionAnswerDto MapUserAnswer(WrittenUserAnswer writtenAnswer)
        {
            var writtenDto = new WrittenAnswerDto
            {
                QuestionId = writtenAnswer.QuestionId,
                Text = writtenAnswer.Value
            };
            return writtenDto;
        }

        private Dtos.Test.QuestionAnswerDto MapUserAnswer(ChoiceUserAnswer choiceAnswer)
        {
            var writtenDto = new ChoiceAnswerDto
            {
                QuestionId = choiceAnswer.QuestionId,
                Value = choiceAnswer.Value
            };
            return writtenDto;
        }

        public Dtos.Wizard.QuestionDto MapToWizardQuestionDto(Question question)
        {
            if (question.Answer is WrittenAnswer writtenAnswer)
            {
                return new Dtos.Wizard.QuestionWithWrittenAnswerDto
                {
                    Id = question.Id,
                    Question = question.Content,
                    Score = question.Answer.MaxScore,
                    Answer = writtenAnswer.Value
                };
            }
            else if (question.Answer is ChoiceAnswer choiceAnswer)
            {
                return new Dtos.Wizard.QuestionWithChoiceAnswersDto
                {
                    Id = question.Id,
                    ChoiceAnswerType = choiceAnswer.ChoiceAnswerType,
                    Categories = question.QuestionCategories.Select(x => x.CategoryId).ToList(),
                    Question = question.Content,
                    Score = question.Answer.MaxScore,
                    AllValidChoicesRequired = choiceAnswer.AllValidChoicesRequired,
                    Choices = choiceAnswer.Choices.Select(x => new Dtos.Wizard.ChoiceDto { Valid = x.Valid, Content = x.Content }).ToList()
                };
            }
            else
                throw new NotSupportedException();
        }

        public Answer MapToAnswer(Dtos.Wizard.QuestionDto dto)
        {
            Answer answer;

            if (dto is Dtos.Wizard.QuestionWithChoiceAnswersDto choiceAnswer)
            {
                var choices = choiceAnswer.Choices
                    .Select(x => new Choice { Content = x.Content, Valid = x.Valid })
                    .ToList();

                answer = new ChoiceAnswer(choices, choiceAnswer.ChoiceAnswerType, choiceAnswer.Score);
            }
            else if (dto is Dtos.Wizard.QuestionWithWrittenAnswerDto writtenAnswer)
            {
                answer = new WrittenAnswer(writtenAnswer.Answer, writtenAnswer.Score);
            }
            else
                throw new Exception("Cannot convert into valid Answer");

            return answer;
        }
    }
}
