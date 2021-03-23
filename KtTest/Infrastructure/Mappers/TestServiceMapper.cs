using KtTest.Dtos.Test;
using KtTest.Models;
using KtTest.Services;
using System;
using System.Linq;

namespace KtTest.Infrastructure.Mappers
{
    public class TestServiceMapper
    {
        private readonly IQuestionServiceMapper questionMapper;
        private readonly IDateTimeProvider dateTimeProvider;

        public TestServiceMapper(IQuestionServiceMapper questionMapper, IDateTimeProvider dateTimeProvider)
        {
            this.questionMapper = questionMapper;
            this.dateTimeProvider = dateTimeProvider;
        }

        public Dtos.Test.TestHeaderDto MapToTestHeaderDto(ScheduledTest scheduledTest)
        {
            return new Dtos.Test.TestHeaderDto
            {
                Id = scheduledTest.Id,
                Name = scheduledTest.TestTemplate.Name,
                StartsAt = scheduledTest.StartDate,
                EndsAt = scheduledTest.EndDate
            };
        }

        public Dtos.Wizard.TestTemplateHeaderDto MapToTestWizardHeaderDto(TestTemplate testTemplate)
        {
            return new Dtos.Wizard.TestTemplateHeaderDto
            {
                Id = testTemplate.Id,
                Name = testTemplate.Name,
                NumberOfQuestions = testTemplate.TestItems.Count
            };
        }

        public Dtos.Wizard.TestTemplateDto MapToTestWizardDto(TestTemplate testTemplate)
        {
            return new Dtos.Wizard.TestTemplateDto
            {
                Name = testTemplate.Name,
                Questions = testTemplate.TestItems
                    .Select(test => questionMapper.MapToWizardQuestionDto(test.Question))
                    .ToList()
            };
        }

        public UserAnswer MapToUserAnswer(Dtos.Test.QuestionAnswerDto answerDto, int testId, int userId)
        {
            if (answerDto is Dtos.Test.WrittenAnswerDto writtenAnswerDto)
            {
                return new WrittenUserAnswer(writtenAnswerDto.Text, testId, answerDto.QuestionId, userId);
            }
            else if (answerDto is Dtos.Test.ChoiceAnswerDto choiceAnswerDto)
            {
                return new ChoiceUserAnswer(choiceAnswerDto.Value, testId, answerDto.QuestionId, userId);
            }
            else
                throw new NotSupportedException();
        }

        public ScheduledTestDto MapToScheduledTestDto(ScheduledTest scheduledTest)
        {
            return new ScheduledTestDto
            {
                Id = scheduledTest.Id,
                Duration = scheduledTest.Duration,
                Name = scheduledTest.TestTemplate.Name,
                StartDate = scheduledTest.StartDate,
                EndDate = scheduledTest.EndDate,
                ScheduledAt = scheduledTest.PublishedAt,
                TestTemplateId = scheduledTest.TestTemplateId,
                Ended = scheduledTest.HasTestComeToEnd(dateTimeProvider)
            };
        }

        public GroupResultsDto MapToGroupResultsDto(GroupResults results)
        {
            return new GroupResultsDto
            {
                Ended = results.Ended,
                MaxTestScore = results.MaxTestScore,
                TestId = results.ScheduledTestId,
                TestName = results.TestName,
                Results = results.Results.Select(MapToUserTestResultDto).ToList()
            };
        }

        private UserTestResultDto MapToUserTestResultDto(UserTestResult result)
        {
            return new UserTestResultDto
            {
                UserId = result.UserId,
                UserScore = result.UserScore,
                Username = result.Username,
                Status = result.Status.ToString(),
            };
        }
    }
}