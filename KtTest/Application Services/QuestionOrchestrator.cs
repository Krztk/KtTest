using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Readers;
using KtTest.Results;
using KtTest.Services;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class QuestionOrchestrator
    {
        private readonly QuestionService questionService;
        private readonly CategoryService categoryService;
        private readonly TestService testService;
        private readonly QuestionServiceMapper questionMapper;
        private readonly QuestionReader questionReader;
        private readonly IUserContext userContext;

        public QuestionOrchestrator(QuestionService questionService,
            CategoryService categoryService,
            TestService testService,
            QuestionServiceMapper questionMapper,
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

        public async Task<PaginatedResult<QuestionDto>> GetQuestions(Pagination pagination)
        {
            var result = await questionService.GetQuestions(pagination.Offset, pagination.Limit);
            return result.MapResult(x => questionMapper.MapToWizardQuestionDto(x));
        }

        public async Task<OperationResult<int>> CreateQuestion(QuestionDto questionDto)
        {
            var result = new OperationResult<int>();
            Answer answer = questionMapper.MapToAnswer(questionDto);

            var categoriesProvided = questionDto.Categories.Count > 0;
            if (categoriesProvided && !categoryService.DoCategoriesExist(questionDto.Categories))
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            result.Data = await questionService.CreateQuestion(questionDto.Question, answer, questionDto.Categories);
            return result;
        }

        public async Task<PaginatedResult<QuestionHeaderDto>> GetQuestionHeaders(Pagination pagination)
        {
            return await questionReader.GetQuestionHeaders(userContext.UserId, pagination.Offset, pagination.Limit);
        }

        public async Task<OperationResult> UpdateQuestion(int questionId, QuestionDto questionDto)
        {
            var result = new OperationResult();

            var isQuestionAuthor = await questionService.IsAuthorOfQuestion(userContext.UserId, questionId);
            if (!isQuestionAuthor)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            if (await testService.HasTestWithQuestionStarted(questionId))
            {
                result.AddFailure(Failure.BadRequest("Cannot edit the question if there is a test which contains it that has already started"));
                return result;
            }

            Answer answer = questionMapper.MapToAnswer(questionDto);
            return await questionService.UpdateQuestion(questionId, questionDto.Question, answer, questionDto.Categories);
        }
    }
}
