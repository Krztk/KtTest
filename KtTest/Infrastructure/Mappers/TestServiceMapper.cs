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

        public Dtos.Test.TestDto MapToTestDto(Test test)
        {
            return new Dtos.Test.TestDto
            {
                Name = test.Name,
                Questions = test.TestItems.Select(x => questionMapper.MapToTestQuestionDto(x.Question)).ToList()
            };
        }

        public Dtos.Test.TestHeaderDto MapToTestHeaderDto(Test x)
        {
            return new Dtos.Test.TestHeaderDto
            {
                Id = x.Id,
                Name = x.Name,
                StartsAt = x.StartDate.GetValueOrDefault(),
                EndsAt = x.EndDate.GetValueOrDefault()
            };
        }

        public Dtos.Wizard.TestHeaderDto MapToTestWizardHeaderDto(Test test)
        {
            return new Dtos.Wizard.TestHeaderDto
            {
                Id = test.Id,
                Name = test.Name,
                NumberOfQuestions = test.TestItems.Count,
                Published = test.PublishedAt.HasValue
            };
        }

        public Dtos.Wizard.TestDto MapToTestWizardDto(Test test)
        {
            return new Dtos.Wizard.TestDto
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
            answer.TestId = testId;
            answer.UserId = userId;

            return answer;
        }

        public TeacherTestResultsDto MapToTeacherTestResultsDto(TestResults results)
        {
            return new TeacherTestResultsDto
            {
                TestId = results.TestId,
                QuestionResults = results.QuestionResults.Select(MapToQuestionResultDto).ToList(),
                TestFinished = results.TestFinished
            };
        }

        public GroupResultsDto MapToGroupResultsDto(GroupResults results)
        {
            return new GroupResultsDto
            {
                Ended = results.Ended,
                NumberOfQuestion = results.NumberOfQuestion,
                TestId = results.TestId,
                TestName = results.TestName,
                Results = results.Results.Select(MapToUserTestResultDto).ToList()
            };
        }

        private QuestionResultDto MapToQuestionResultDto(QuestionResult questionResult)
        {
            return new QuestionResultDto
            {
                NumberOfValidAnswers = questionResult.NumberOfValidAnswers,
                TotalNumberOfAnswers = questionResult.TotalNumberOfAnswers,
                QuestionId = questionResult.QuestionId,
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