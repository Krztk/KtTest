using KtTest.Dtos.Test;
using KtTest.Infrastructure.Data;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Results;
using KtTest.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Readers
{
    public class TestReader
    {
        private readonly ReadOnlyAppDbContext dbContext;
        private readonly QuestionServiceMapper questionMapper;
        private readonly TestServiceMapper testMapper;
        private readonly IDateTimeProvider dateTimeProvider;

        public TestReader(ReadOnlyAppDbContext dbContext, QuestionServiceMapper questionMapper, TestServiceMapper testMapper, IDateTimeProvider dateTimeProvider)
        {
            this.dbContext = dbContext;
            this.questionMapper = questionMapper;
            this.testMapper = testMapper;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<OperationResult<Dtos.Test.TestDto>> GetTest(int testId)
        {
            var test = await dbContext.ScheduledTests
                .Include(x => x.TestTemplate)
                    .ThenInclude(x => x.TestItems)
                        .ThenInclude(x => x.Question)
                            .ThenInclude(x => x.Answer)
                                .ThenInclude(x => ((ChoiceAnswer)x).Choices)
                .Where(x => x.Id == testId)
                .FirstOrDefaultAsync();

            var result = new OperationResult<Dtos.Test.TestDto>();
            if (test == null)
            {
                result.AddFailure(Failure.NotFound());
                return result;
            }

            var dto = new TestDto
            {
                Name = test.TestTemplate.Name,
                Duration = test.Duration,
                Questions = test.TestTemplate.TestItems.Select(x => questionMapper.MapToTestQuestionDto(x.Question)).ToList()
            };

            result.Data = dto;
            return result;
        }

        public PaginatedResult<Dtos.Test.TestHeaderDto> GetAvailableAndUpcomingTests(int userId, int offset, int limit)
        {
            var tests = dbContext.UserTests
                .Include(x => x.ScheduledTest)
                    .ThenInclude(x => x.TestTemplate)
                .Where(x => x.UserId == userId && x.ScheduledTest.EndDate > dateTimeProvider.UtcNow)
                .Skip(offset).Take(limit + 1)
                .Select(x => x.ScheduledTest)
                .Select(testMapper.MapToTestHeaderDto)
                .ToList();

            var result = new PaginatedResult<Dtos.Test.TestHeaderDto>();
            result.Data = new Paginated<Dtos.Test.TestHeaderDto>(limit, tests);
            return result;
        }

        public OperationResult<Dtos.Wizard.TestTemplateDto> GetTestTemplate(int id, int authorId)
        {
            var result = new OperationResult<Dtos.Wizard.TestTemplateDto>();
            var testTemplate = dbContext.TestTemplates
                .Where(x => x.Id == id && x.AuthorId == authorId)
                .Include(x => x.TestItems)
                    .ThenInclude(x => x.Question)
                        .ThenInclude(x => x.Answer)
                            .ThenInclude(x => ((ChoiceAnswer)x).Choices)
            .Select(testMapper.MapToTestWizardDto)
            .FirstOrDefault();

            if (testTemplate == null)
            {
                result.AddFailure(Failure.NotFound());
                return result;
            }

            result.Data = testTemplate;
            return result;
        }

        public async Task<OperationResult<Dtos.Test.TestResultsDto>> GetTestResultsDto(int testId)
        {
            var testAndQuestions = await dbContext.ScheduledTests
                .Include(x => x.TestTemplate)
                    .ThenInclude(x => x.TestItems)
                        .ThenInclude(x => x.Question)
                            .ThenInclude(x => x.Answer)
                                .ThenInclude(x => ((ChoiceAnswer)x).Choices)
                .Where(x => x.Id == testId)
                .Join(dbContext.UserAnswers, x => x.Id, y => y.ScheduledTestId, (x, y) => new { x.TestTemplate, UserAnswer = y })
                .ToListAsync();

            var result = new OperationResult<Dtos.Test.TestResultsDto>();
            if (testAndQuestions.Count == 0)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }
            var test = testAndQuestions[0].TestTemplate;
            var userAnswers = testAndQuestions.Select(x => x.UserAnswer).ToDictionary(x => x.QuestionId);
            var questions = test.TestItems.Select(x => x.Question).ToList();

            var questionsWithResult = new List<QuestionWithResultDto>();
            foreach (var question in questions)
            {
                QuestionWithResultDto questionWithResultDto = questionMapper.MapToTestQuestionWithResultDto(question, userAnswers[question.Id]);
                questionsWithResult.Add(questionWithResultDto);
            }

            var testResultsDto = new TestResultsDto
            {
                Name = test.Name,
                QuestionsWithResult = questionsWithResult
            };

            result.Data = testResultsDto;
            return result;
        }

        public PaginatedResult<Dtos.Wizard.TestTemplateHeaderDto> GetTestTemplateHeaders(int authorId, int offset, int limit)
        {
            var tests = dbContext.TestTemplates
                .Where(x => x.AuthorId == authorId)
                .Include(x => x.TestItems)
                .Skip(offset)
                .Take(limit + 1)
                .Select(testMapper.MapToTestWizardHeaderDto);

            var result = new PaginatedResult<Dtos.Wizard.TestTemplateHeaderDto>();
            result.Data = new Paginated<Dtos.Wizard.TestTemplateHeaderDto>(limit, tests);
            return result;
        }

        public OperationResult<List<QuestionAnswerDto>> GetUserAnswers(int userId, int testId)
        {
            var result = new OperationResult<List<QuestionAnswerDto>>();
            var userAnswers = dbContext.UserAnswers
                .Where(x => x.ScheduledTestId == testId && x.UserId == userId)
                .OrderBy(x => x.QuestionId)
                .Select(questionMapper.MapToTestQuestionAnswerDto)
                .ToList();

            result.Data = userAnswers;
            return result;
        }
    }
}
