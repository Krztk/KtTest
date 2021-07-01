using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Readers;
using KtTest.Results;
using KtTest.Results.Errors;
using KtTest.Services;
using System.Threading.Tasks;
using KtTest.Extensions;

namespace KtTest.Application_Services
{
    public class QuestionOrchestrator
    {
        private readonly QuestionService questionService;
        private readonly CategoryService categoryService;
        private readonly TestService testService;
        private readonly IQuestionServiceMapper questionMapper;
        private readonly QuestionReader questionReader;
        private readonly IUserContext userContext;

        public QuestionOrchestrator(QuestionService questionService,
            CategoryService categoryService,
            TestService testService,
            IQuestionServiceMapper questionMapper,
            QuestionReader questionReader,
            IUserContext userContext)
        {
            this.questionService = questionService;
            this.categoryService = categoryService;
            this.testService = testService;
            this.questionMapper = questionMapper;
            this.questionReader = questionReader;
            this.userContext = userContext;
        }

        public OperationResult<Paginated<QuestionDto>> GetQuestions(Pagination pagination)
        {
            return questionReader.GetQuestions(userContext.UserId, pagination.Offset, pagination.Limit);
        }

        public async Task<OperationResult<QuestionDto>> GetQuestion(int questionId)
        {
            return await questionReader.GetQuestion(userContext.UserId, questionId);
        }

        public async Task<OperationResult<int>> CreateQuestion(QuestionDto questionDto)
        {
            Answer answer = questionMapper.MapToAnswer(questionDto);

            var areCategoriesProvided = questionDto.Categories.Count > 0;
            if (areCategoriesProvided && !categoryService.DoCategoriesExist(questionDto.Categories))
            {
                return new BadRequestError();
            }

            return await questionService.CreateQuestion(questionDto.Question, answer, questionDto.Categories);
        }

        public async Task<OperationResult<Paginated<QuestionHeaderDto>>> GetQuestionHeaders(Pagination pagination)
        {
            return await questionReader.GetQuestionHeaders(userContext.UserId, pagination.Offset, pagination.Limit);
        }

        public async Task<OperationResult<Unit>> UpdateQuestion(int questionId, QuestionDto questionDto)
        {
            return await CanUpdateQuestion(questionId).Then(x =>
            {
                Answer answer = questionMapper.MapToAnswer(questionDto);
                return questionService.UpdateQuestion(questionId, questionDto.Question, answer, questionDto.Categories);
            });
        }

        public async Task<OperationResult<Unit>> DeleteQuestion(int questionId)
        {
            return await CanDeleteQuestion(questionId)
                .Then(x => questionService.DeleteQuestion(questionId));
        }

        private async Task<OperationResult<Unit>> CanUpdateQuestion(int questionId)
        {
            var isQuestionAuthor = await questionService.IsAuthorOfQuestion(userContext.UserId, questionId);
            if (!isQuestionAuthor)
            {
                return new BadRequestError();
            }

            if (await testService.HasTestWithQuestionStarted(questionId))
            {
                return new BadRequestError("Cannot update the question if there is a test, which contains it, that has already started");
            }

            return OperationResult.Ok;
        }

        private async Task<OperationResult<Unit>> CanDeleteQuestion(int questionId)
        {
            var isQuestionAuthor = await questionService.IsAuthorOfQuestion(userContext.UserId, questionId);
            if (!isQuestionAuthor)
            {
                return new BadRequestError();
            }

            if (await testService.IsQuestionIncludedInTest(questionId))
            {
                return new BadRequestError("Cannot delete question used in test");
            }

            return OperationResult.Ok;
        }
    }
}
