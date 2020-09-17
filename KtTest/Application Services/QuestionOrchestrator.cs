using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Results;
using KtTest.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class QuestionOrchestrator
    {
        private readonly QuestionService questionService;
        private readonly CategoryService categoryService;
        private readonly QuestionServiceMapper questionMapper;

        public QuestionOrchestrator(QuestionService questionService, CategoryService categoryService, QuestionServiceMapper questionMapper)
        {
            this.questionService = questionService;
            this.categoryService = categoryService;
            this.questionMapper = questionMapper;
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
    }
}
