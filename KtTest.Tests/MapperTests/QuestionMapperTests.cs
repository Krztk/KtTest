using FluentAssertions;
using KtTest.Dtos.Test;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace KtTest.Tests.MapperTests
{
    public class QuestionMapperTests
    {
        Question questionWithWrittenAnswer;
        public QuestionMapperTests()
        {
            float score = 2f;
            int authorId = 1;

            questionWithWrittenAnswer = new Question("QuestionContent",
                new WrittenAnswer("Answer Content", score), authorId);
            questionWithWrittenAnswer.Id = 5;
        }

        [Fact]
        public void MapToTestQuestionDto_QuestionWithWrittenAnswer_ValidDto()
        {
            //arrange
            var expectedDto = new QuestionWithWrittenAnswerDto
            {
                Id = questionWithWrittenAnswer.Id,
                Question = questionWithWrittenAnswer.Content
            };

            //act
            var mapper = new QuestionServiceMapper();
            var dto = mapper.MapToTestQuestionDto(questionWithWrittenAnswer);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Theory]
        [InlineData(ChoiceAnswerType.SingleChoice)]
        [InlineData(ChoiceAnswerType.MultipleChoice)]
        public void MapToTestQuestionDto_QuestionWithChoiceAnswers_ValidDto(ChoiceAnswerType choiceAnswerType)
        {
            //arrange
            List<Choice> choices = GetChoices();
            Question question = GetQuestion(choiceAnswerType, choices);
            var expectedDto = new QuestionWithChoiceAnswersDto
            {
                Id = question.Id,
                Question = question.Content,
                ChoiceAnswerType = choiceAnswerType,
                Choices = choices.Select(x => x.Content).ToList()
            };

            //act
            var mapper = new QuestionServiceMapper();
            var dto = mapper.MapToTestQuestionDto(question);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public void MapToTestQuestionAnswerDto_WrittenUserAnswer_ValidDto()
        {
            //arrange
            int questionId = 4;
            string userAnswerContent = "user answer value";
            UserAnswer userAnswer = GetWrittenUserAnswer(questionId, userAnswerContent);
            var expectedDto = new WrittenAnswerDto
            {
                QuestionId = questionId,
                Text = userAnswerContent
            };

            //act
            var mapper = new QuestionServiceMapper();
            var dto = mapper.MapToTestQuestionAnswerDto(userAnswer);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public void MapToTestQuestionAnswerDto_ChoiceUserAnswer_ValidDto()
        {
            //arrange
            int questionId = 2;
            int numericValue = 5;
            UserAnswer userAnswer = GetChoiceUserAnswer(questionId, numericValue);
            var expectedDto = new ChoiceAnswerDto
            {
                QuestionId = questionId,
                Value = numericValue
            };
            //act
            var mapper = new QuestionServiceMapper();
            var dto = mapper.MapToTestQuestionAnswerDto(userAnswer);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public void MapToTestQuestionWithResultDto_QuestionWithWrittenAnswer_ValidDto()
        {
            //arrange
            var userAnswerContent = "my answer";
            var userAnswer = GetWrittenUserAnswer(questionWithWrittenAnswer.Id, userAnswerContent);
            QuestionWithResultDto expectedDto = new QuestionWithWrittenResultDto
            {
                Question = questionWithWrittenAnswer.Content,
                QuestionId = questionWithWrittenAnswer.Id,
                CorrectAnswer = ((WrittenAnswer)questionWithWrittenAnswer.Answer).Value,
                UserAnswer = userAnswerContent
            };
            //act
            var mapper = new QuestionServiceMapper();
            var dto = mapper.MapToTestQuestionWithResultDto(questionWithWrittenAnswer, userAnswer);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public void MapToTestQuestionWithResultDto_QuestionWithChoiceAnswers_ValidDto()
        {
            //arrange
            var userNumericAnswer = 5;
            var choices = GetChoices();
            var question = GetQuestion(ChoiceAnswerType.MultipleChoice, choices);
            var userAnswer = GetChoiceUserAnswer(question.Id, userNumericAnswer);
            var expectedChoiceDtos = new List<ChoiceDto> {
                new ChoiceDto {Correct = choices[0].Valid, Value = choices[0].Content, UserAnswer = true },
                new ChoiceDto {Correct = choices[1].Valid, Value = choices[1].Content, UserAnswer = false },
                new ChoiceDto {Correct = choices[2].Valid, Value = choices[2].Content, UserAnswer = true },
            };
                
            QuestionWithResultDto expectedDto = new QuestionWithChoiceAnswerResultDto
            {
                Question = question.Content,
                QuestionId = question.Id,
                Choices = expectedChoiceDtos
            };
            //act
            var mapper = new QuestionServiceMapper();
            var dto = mapper.MapToTestQuestionWithResultDto(question, userAnswer);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public void MapToWizardQuestionDto_QuestionWithWrittenAnswers_ValidDto()
        {
            //arrange
            var expectedDto = new Dtos.Wizard.QuestionWithWrittenAnswerDto
            {
                Id = questionWithWrittenAnswer.Id,
                Answer = ((WrittenAnswer)questionWithWrittenAnswer.Answer).Value,
                Question = questionWithWrittenAnswer.Content,
                Score = questionWithWrittenAnswer.Answer.MaxScore,
                Categories = new List<int>()
            };

            //act
            var mapper = new QuestionServiceMapper();
            var dto = mapper.MapToWizardQuestionDto(questionWithWrittenAnswer);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Theory]
        [InlineData(ChoiceAnswerType.SingleChoice, true)]
        [InlineData(ChoiceAnswerType.SingleChoice, false)]
        [InlineData(ChoiceAnswerType.MultipleChoice, true)]
        [InlineData(ChoiceAnswerType.MultipleChoice, false)]
        public void MapToWizardQuestionDto_QuestionWithChoiceAnswers_ValidDto(ChoiceAnswerType choiceAnswerType, bool allRequired)
        {
            //arrange
            var choices = GetChoices();
            var question = GetQuestion(choiceAnswerType, choices, allRequired);
            var expectedDto = new Dtos.Wizard.QuestionWithChoiceAnswersDto
            {
                Id = question.Id,
                Choices = ((ChoiceAnswer)question.Answer)
                    .Choices.Select(x=> new Dtos.Wizard.ChoiceDto {Content = x.Content, Valid = x.Valid })
                    .ToList(),
                AllValidChoicesRequired = allRequired,
                ChoiceAnswerType = choiceAnswerType,
                Question = question.Content,
                Score = question.Answer.MaxScore,
                Categories = new List<int>()
            };

            //act
            var mapper = new QuestionServiceMapper();
            var dto = mapper.MapToWizardQuestionDto(question);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public void MapToAnswer_QuestionWithWritenAnswerDto_ValidAnswer()
        {
            //arrange
            var answerValue = "answer content";
            var score = 5f;
            Dtos.Wizard.QuestionDto dto = new Dtos.Wizard.QuestionWithWrittenAnswerDto
            {
                Id = 1,
                Answer = answerValue,
                Categories = new List<int>(),
                Question = "Question content",
                Score = score
            };
            var expectedAnswer = new WrittenAnswer(answerValue, score);

            //act
            var mapper = new QuestionServiceMapper();
            var answer = mapper.MapToAnswer(dto);

            //assert
            answer.Should().BeEquivalentTo(expectedAnswer);
        }

        [Theory]
        [InlineData(ChoiceAnswerType.SingleChoice, true)]
        [InlineData(ChoiceAnswerType.SingleChoice, false)]
        [InlineData(ChoiceAnswerType.MultipleChoice, true)]
        [InlineData(ChoiceAnswerType.MultipleChoice, false)]
        public void MapToAnswer_QuestionWithChoiceAnswersDto_ValidAnswer(ChoiceAnswerType choiceAnswerType, bool allRequired)
        {
            //arrange
            var score = 5f;
            Dtos.Wizard.QuestionDto dto = new Dtos.Wizard.QuestionWithChoiceAnswersDto
            {
                Id = 1,
                ChoiceAnswerType = choiceAnswerType,
                AllValidChoicesRequired = allRequired,
                Choices = new List<Dtos.Wizard.ChoiceDto>
                {
                    new Dtos.Wizard.ChoiceDto { Content = "1", Valid = true },
                    new Dtos.Wizard.ChoiceDto { Content = "2", Valid = false },
                },
                Categories = new List<int>(),
                Question = "Question content",
                Score = score
            };
            var expectedChoices = new List<Choice>
            {
                new Choice
                {
                    Content = "1",
                    Valid = true
                },
                new Choice
                {
                    Content = "2",
                    Valid = false
                }
            };
            var expectedAnswer = new ChoiceAnswer(expectedChoices, choiceAnswerType, score, allRequired);

            //act
            var mapper = new QuestionServiceMapper();
            var answer = mapper.MapToAnswer(dto);

            //assert
            answer.Should().BeEquivalentTo(expectedAnswer);
        }

        private static Question GetQuestion(ChoiceAnswerType choiceAnswerType, List<Choice> choices, bool allRequired = true)
        {
            int questionId = 5;
            var authorId = 1;
            var score = 1f;
            var answer = new ChoiceAnswer(choices, choiceAnswerType, score, allRequired);
            var question = new Question("Question content", answer, authorId);
            question.Id = questionId;
            return question;
        }

        private static List<Choice> GetChoices()
        {
            return new List<Choice>
            {
                new Choice
                {
                    Content = "1",
                    Valid = true,
                },
                new Choice
                {
                    Content = "2",
                    Valid = false,
                },
                new Choice
                {
                    Content = "3",
                    Valid = false,
                }
            };
        }

        private static WrittenUserAnswer GetWrittenUserAnswer(int questionId, string answerValue)
        {
            int scheduledTestId = 3;
            int userId = 2;
            return new WrittenUserAnswer(answerValue, scheduledTestId, questionId, userId);
        }

        private static ChoiceUserAnswer GetChoiceUserAnswer(int questionId, int numericValue)
        {
            int userId = 2;
            int scheduledTestId = 3;
            return new ChoiceUserAnswer(numericValue, scheduledTestId, questionId, userId);
        }
    }
}
