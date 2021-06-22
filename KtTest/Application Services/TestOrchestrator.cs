using KtTest.Dtos.Test;
using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Readers;
using KtTest.Results;
using KtTest.Results.Errors;
using KtTest.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class TestOrchestrator
    {
        private readonly QuestionService questionService;
        private readonly TestService testService;
        private readonly GroupService groupService;
        private readonly TestReader testReader;
        private readonly TestServiceMapper testMapper;
        private readonly IUserContext userContext;
        private readonly IDateTimeProvider dateTimeProvider;

        public TestOrchestrator(QuestionService questionService,
            TestService testService,
            GroupService groupService,
            TestReader testReader,
            TestServiceMapper testMapper,
            IUserContext userContext,
            IDateTimeProvider dateTimeProvider)
        {
            this.questionService = questionService;
            this.testService = testService;
            this.groupService = groupService;
            this.testReader = testReader;
            this.testMapper = testMapper;
            this.userContext = userContext;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<OperationResult<int>> CreateTestTemplate(Dtos.Wizard.CreateTestTemplateDto testDto)
        {
            var doQuestionsExist = questionService.DoQuestionsExist(testDto.QuestionIds);
            if (!doQuestionsExist)
            {
                return new BadRequestError();
            }

            return await testService.CreateTestTemplate(testDto.Name, testDto.QuestionIds);
        }

        public async Task<OperationResult<Paginated<ScheduledTestDto>>> GetScheduledTests(Pagination pagination)
        {
            return await testReader.GetScheduledTest(userContext.UserId, pagination.Offset, pagination.Limit);
        }

        public OperationResult<Paginated<Dtos.Test.TestHeaderDto>> GetAvailableAndUpcomingTests(Pagination pagination)
        {
            return testReader.GetAvailableAndUpcomingTests(userContext.UserId, pagination.Offset, pagination.Limit);
        }

        public OperationResult<Paginated<Dtos.Wizard.TestTemplateHeaderDto>> GetTestTemplates(Pagination pagination)
        {
            pagination ??= new Pagination(offset: 0, limit: 25);
            return testReader.GetTestTemplateHeaders(userContext.UserId, pagination.Offset, pagination.Limit);
        }

        public OperationResult<Dtos.Wizard.TestTemplateDto> GetTestTemplate(int id)
        {
            return testReader.GetTestTemplate(id, userContext.UserId);
        }

        public async Task<OperationResult<TestResultsDto>> GetTestResult(int testId)
        {
            var hasAccess = await testService.HasUserTakenTest(testId, userContext.UserId);
            if (!hasAccess)
            {
                return new AuthorizationError();
            }

            var testEndedResult = await testService.HasTestComeToEnd(testId);
            if (!testEndedResult.Succeeded)
            {
                return testEndedResult.Error;
            }

            bool hasEnded = testEndedResult;
            if (!hasEnded)
            {
                return new BadRequestError("Test hasn't come to end yet");
            }

            return await testReader.GetTestResultsDto(testId, userContext.UserId);
        }

        public async Task<OperationResult<GroupResultsDto>> GetTestResultTeacher(int testId)
        {
            var result = await testService.GetGroupResults(testId);
            return result.Then<GroupResultsDto>(x => testMapper.MapToGroupResultsDto(x));
        }

        public async Task<OperationResult<Dtos.Test.TestDto>> GetTest(int id)
        {
            var canGetTest = await testService.CanGetTest(id, userContext.UserId);

            if (!canGetTest.Succeeded)
            {
                return canGetTest.Error;
            }

            if (!canGetTest.Data)
            {
                return new BadRequestError();
            }

            testService.MarkTestAsStartedIfItHasntBeenMarkedAlready(id, userContext.UserId);
            return await testReader.GetTest(id);
        }

        public async Task<OperationResult<Unit>> AddUserAnswers(int testId, SendTestAnswersDto dto)
        {
            int userId = userContext.UserId;
            List<UserAnswer> answers = dto.Answers
                .Select(x => testMapper.MapToUserAnswer(x, testId, userId))
                .ToList();

            if (answers.Count == 0)
            {
                return new BadRequestError();
            }

            return await testService.AddUserAnswers(testId, answers);
        }

        public async Task<OperationResult<List<QuestionAnswerDto>>> GetUserAnswers(int testId)
        {
            var testTaken = await testService.HasUserTakenTest(testId, userContext.UserId);
            if (!testTaken)
            {
                return new BadRequestError();
            }

            return testReader.GetUserAnswers(userContext.UserId, testId);
        }

        public async Task<OperationResult<Unit>> ScheduleTest(int testId, PublishTestDto publishTestDto)
        {
            if (publishTestDto.StartDate >= publishTestDto.EndDate
                && dateTimeProvider.UtcNow >= publishTestDto.StartDate)
            {
                return new BadRequestError();
            }

            bool isMember = await groupService.IsUserMemberOfGroup(userContext.UserId, publishTestDto.GroupId);
            if (!isMember)
            {
                return new BadRequestError();
            }

            var students = await groupService.GetStudentsFromGroup(publishTestDto.GroupId);

            return await testService.ScheduleTest(testId,
                publishTestDto.StartDate,
                publishTestDto.EndDate,
                publishTestDto.DurationInMinutes,
                students);
        }
    }
}