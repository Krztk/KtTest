using KtTest.Dtos.Test;
using KtTest.Models;
using System;
using System.Linq;

namespace KtTest.Infrastructure.Mappers
{
    public class TestServiceMapper
    {
        private readonly QuestionServiceMapper questionMapper;

        public TestServiceMapper(QuestionServiceMapper questionMapper)
        {
            this.questionMapper = questionMapper;
        }

        public Dtos.Test.TestDto MapToTestDto(TestTemplate test)
        {
            return new Dtos.Test.TestDto
            {
                Name = test.Name,
                Questions = test.TestItems.Select(x => questionMapper.MapToTestQuestionDto(x.Question)).ToList()
            };
        }

        public Dtos.Test.TestHeaderDto MapToTestHeaderDto(ScheduledTest x)
        {
            return new Dtos.Test.TestHeaderDto
            {
                Id = x.Id,
                Name = x.TestTemplate.Name,
                StartsAt = x.StartDate,
                EndsAt = x.EndDate
            };
        }

        public Dtos.Wizard.TestTemplateHeaderDto MapToTestWizardHeaderDto(TestTemplate test)
        {
            return new Dtos.Wizard.TestTemplateHeaderDto
            {
                Id = test.Id,
                Name = test.Name,
                NumberOfQuestions = test.TestItems.Count
            };
        }

        public Dtos.Wizard.TestTemplateDto MapToTestWizardDto(TestTemplate test)
        {
            return new Dtos.Wizard.TestTemplateDto
            {
                Name = test.Name,
                Questions = test.TestItems
                    .Select(test => questionMapper.MapToWizardQuestionDto(test.Question))
                    .ToList()
            };
        }

        public UserAnswer MapToUserAnswer(Dtos.Test.QuestionAnswerDto answerDto, int testId, int userId)
        {
            UserAnswer answer;
            if (answerDto is Dtos.Test.WrittenAnswerDto writtenAnswerDto)
            {
                answer = new WrittenUserAnswer(writtenAnswerDto.Text);

            }
            else if (answerDto is Dtos.Test.ChoiceAnswerDto choiceAnswerDto)
            {
                answer = new ChoiceUserAnswer(choiceAnswerDto.Value);
            }
            else
                throw new NotSupportedException();

            answer.QuestionId = answerDto.QuestionId;
            answer.ScheduledTestId = testId;
            answer.UserId = userId;

            return answer;
        }

        public GroupResultsDto MapToGroupResultsDto(GroupResults results)
        {
            return new GroupResultsDto
            {
                Ended = results.Ended,
                NumberOfQuestion = results.NumberOfQuestion,
                TestId = results.ScheduledTestId,
                TestName = results.TestName,
                Results = results.Results.Select(MapToUserTestResultDto).ToList()
            };
        }

        private UserTestResultDto MapToUserTestResultDto(UserTestResult result)
        {
            return new UserTestResultDto
            {
                Id = result.Id,
                NumberOfValidAnswers = result.NumberOfValidAnswers,
                Username = result.Username,
            };
        }
    }
}