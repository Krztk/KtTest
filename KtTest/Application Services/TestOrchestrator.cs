using KtTest.Dtos.Test;
using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Readers;
using KtTest.Results;
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

        public async Task<OperationResult<int>> CreateTestTemplate(Dtos.Wizard.CreateTestDto testDto)
        {
            var result = new OperationResult<int>();

            var doQuestionsExist = questionService.DoQuestionsExist(testDto.QuestionIds);
            if (!doQuestionsExist)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            result = await testService.CreateTestTemplate(testDto.Name, testDto.QuestionIds);
            return result;
        }

        public async Task<PaginatedResult<ScheduledTestDto>> GetScheduledTests(Pagination pagination)
        {
            return await testReader.GetScheduledTest(userContext.UserId, pagination.Offset, pagination.Limit);
        }

        public PaginatedResult<Dtos.Test.TestHeaderDto> GetAvailableAndUpcomingTests(Pagination pagination)
        {
            return testReader.GetAvailableAndUpcomingTests(userContext.UserId, pagination.Offset, pagination.Limit);
        }

        public PaginatedResult<Dtos.Wizard.TestTemplateHeaderDto> GetTestTemplates(Pagination pagination)
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

            return await testReader.GetTestResultsDto(testId);
        }

        public async Task<OperationResult<GroupResultsDto>> GetTestResultTeacher(int testId)
        {
            var result = await testService.GetGroupResults(testId);
            return result.MapResult(testMapper.MapToGroupResultsDto);
        }

        public async Task<OperationResult<Dtos.Test.TestDto>> GetTest(int id)
        {
            var canGetTestResult = await testService.CanGetTest(id, userContext.UserId);
            if (!canGetTestResult.Succeeded)
            {
                return canGetTestResult.MapResult<Dtos.Test.TestDto>();
            }

            if (!canGetTestResult.Data)
            {
                var result = new OperationResult<Dtos.Test.TestDto>();
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            testService.MarkTestAsStartedIfItHasntBeenMarkedAlready(id, userContext.UserId);
            return await testReader.GetTest(id);
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
            var testTaken = await testService.HasUserTakenTest(testId, userContext.UserId);
            if (!testTaken)
            {
                var result = new OperationResult<List<QuestionAnswerDto>>();
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            return testReader.GetUserAnswers(userContext.UserId, testId);
        }

        public async Task<OperationResult> ScheduleTest(int testId, PublishTestDto publishTestDto)
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

            return await testService.ScheduleTest(testId,
                publishTestDto.StartDate,
                publishTestDto.EndDate,
                publishTestDto.DurationInMinutes,
                students);
        }
    }
}