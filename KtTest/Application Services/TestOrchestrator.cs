using KtTest.Dtos.Test;
using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Results;
using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class TestOrchestrator
    {
        private readonly QuestionService questionService;
        private readonly TestService testService;
        private readonly GroupService groupService;
        private readonly QuestionServiceMapper questionMapper;
        private readonly TestServiceMapper testMapper;
        private readonly IUserContext userContext;
        private readonly IDateTimeProvider dateTimeProvider;

        public TestOrchestrator(QuestionService questionService,
            TestService testService,
            GroupService groupService,
            QuestionServiceMapper questionMapper,
            TestServiceMapper testMapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider)
        {
            this.questionService = questionService;
            this.testService = testService;
            this.groupService = groupService;
            this.questionMapper = questionMapper;
            this.testMapper = testMapper;
            this.userContext = userContext;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<OperationResult<int>> CreateTest(Dtos.Wizard.CreateTestDto testDto)
        {
            var result = new OperationResult<int>();

            var doQuestionsExist = questionService.DoQuestionsExist(testDto.QuestionIds);
            if (!doQuestionsExist)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            result = await testService.CreateTest(testDto.Name, testDto.QuestionIds);
            return result;
        }

        public async Task<PaginatedResult<Dtos.Test.TestHeaderDto>> GetAvailableAndUpcomingTests(Pagination pagination)
        {
            var result = await testService.GetAvailableAndUpcomingTests(pagination.Offset, pagination.Limit);
            return result.MapResult(testMapper.MapToTestHeaderDto);
        }

        public async Task<PaginatedResult<Dtos.Wizard.TestHeaderDto>> GetTests(Pagination pagination)
        {
            pagination ??= new Pagination(offset: 0, limit: 25);
            var result = await testService.GetTests(pagination.Offset, pagination.Limit);
            return result.MapResult(testMapper.MapToTestWizardHeaderDto);
        }

        public async Task<OperationResult<Dtos.Wizard.TestDto>> GetTestWizard(int id)
        {
            var result = await testService.GetTest(id, userContext.UserId);
            return result.MapResult(testMapper.MapToTestWizardDto);
        }

        public async Task<OperationResult<TestResultsDto>> GetTestResult(int testId)
        {
            var result = new OperationResult<TestResultsDto>();
            var hasAccess = await testService.HasUserTakenTest(testId, userContext.UserId);
            if (!hasAccess)
            {
                result.AddFailure(Failure.Unauthorized());
                return result;
            }

            var testEndedResult = await testService.HasTestComeToEnd(testId);
            if (!testEndedResult.Succeeded)
                return testEndedResult.MapResult<TestResultsDto>();

            if (!testEndedResult.Data)
            {
                result.AddFailure(Failure.BadRequest("Test hasn't come to end yet"));
                return result;
            }

            var getTestResult = await testService.GetTest(testId);
            if (!getTestResult.Succeeded)
            {
                return getTestResult.MapResult<TestResultsDto>();
            }
            var test = getTestResult.Data;

            var getAnswersResult = await testService.GetAnswers(testId, userContext.UserId);
            if (!getAnswersResult.Succeeded)
            {
                return getAnswersResult.MapResult<TestResultsDto>();
            }

            var userAnswers = getAnswersResult.Data.ToDictionary(x => x.QuestionId);
            var questionsWithResult = new List<QuestionWithResultDto>();
            foreach (var testItem in test.TestItems)
            {

                QuestionWithResultDto dto = questionMapper.MapToTestQuestionWithResultDto(testItem.Question, userAnswers[testItem.QuestionId]);
                questionsWithResult.Add(dto);
            }

            var testResultsDto = new TestResultsDto
            {
                Name = test.Name,
                QuestionsWithResult = questionsWithResult
            };

            result.Data = testResultsDto;
            return result;
        }

        public async Task<OperationResult<GroupResultsDto>> GetTestResultTeacher(int testId)
        {
            var result = await testService.GetGroupResults(testId);
            return result.MapResult(testMapper.MapToGroupResultsDto);
        }

        public async Task<OperationResult<Dtos.Test.TestDto>> GetTest(int id)
        {
            var result = await testService.GetTestForStudent(id, userContext.UserId);
            return result.MapResult(testMapper.MapToTestDto);
        }

        public async Task<OperationResult> AddUserAnswers(int testId, SendTestAnswersDto dto)
        {
            int userId = userContext.UserId;
            List<UserAnswer> answers = dto.Answers
                .Select(x => testMapper.MapToUserAnswer(x, testId, userId))
                .ToList();

            var result = new OperationResult();

            if (answers.Count == 0)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            return await testService.AddUserAnswers(testId, answers);
        }

        public async Task<OperationResult<List<QuestionAnswerDto>>> GetUserAnswers(int testId)
        {
            int userId = userContext.UserId;
            var result = await testService.GetAnswers(testId, userId);
            return result.MapResult(x => x.Select(y => questionMapper.MapToTestQuestionAnswerDto(y)).ToList());
        }

        public async Task<OperationResult<List<UserAnswer>>> GetUserAnswers(int testId, int userId)
        {
            var result = await testService.GetAnswers(testId, userId);
            return result;
        }

        public async Task<OperationResult> PublishTest(int testId, PublishTestDto publishTestDto)
        {
            var result = new OperationResult();
            if (publishTestDto.StartDate >= publishTestDto.EndDate
                && dateTimeProvider.UtcNow >= publishTestDto.StartDate)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            bool isMember = await groupService.IsUserMemberOfGroup(userContext.UserId, publishTestDto.GroupId);
            if (!isMember)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            var students = await groupService.GetStudentsFromGroup(publishTestDto.GroupId);

            return await testService.PublishTest(testId,
                publishTestDto.StartDate,
                publishTestDto.EndDate,
                publishTestDto.DurationInMinutes,
                students);
        }
    }
}