using KtTest.Dtos.Test;
using KtTest.Infrastructure.Data;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Results;
using KtTest.Results.Errors;
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

            if (test == null)
            {
                return new DataNotFoundError();
            }

            var dto = new TestDto
            {
                Name = test.TestTemplate.Name,
                Duration = test.Duration,
                Questions = test.TestTemplate.TestItems.Select(x => questionMapper.MapToTestQuestionDto(x.Question)).ToList()
            };

            return dto;
        }

        public async Task<OperationResult<Paginated<ScheduledTestDto>>> GetScheduledTest(int userId, int offset, int limit)
        {
            var tests = await dbContext.ScheduledTests
                .Include(x => x.TestTemplate)
                .Include(x => x.UserTests)
                .Where(x => x.TestTemplate.AuthorId == userId)
                .Skip(offset).Take(limit + 1)
                .ToListAsync();

            return new Paginated<ScheduledTestDto>(limit, tests.Select(testMapper.MapToScheduledTestDto));
        }

        public OperationResult<Paginated<Dtos.Test.TestHeaderDto>> GetAvailableAndUpcomingTests(int userId, int offset, int limit)
        {
            var tests = dbContext.UserTests
                .Include(x => x.ScheduledTest)
                    .ThenInclude(x => x.TestTemplate)
                .Where(x => x.UserId == userId && x.ScheduledTest.EndDate > dateTimeProvider.UtcNow)
                .Skip(offset).Take(limit + 1)
                .Select(x => x.ScheduledTest)
                .Select(testMapper.MapToTestHeaderDto)
                .ToList();

            return new Paginated<Dtos.Test.TestHeaderDto>(limit, tests);
        }

        public OperationResult<Dtos.Wizard.TestTemplateDto> GetTestTemplate(int id, int authorId)
        {
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
                return new DataNotFoundError();
            }

            return testTemplate;
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

            if (testAndQuestions.Count == 0)
            {
                return new BadRequestError();
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

            return testResultsDto;
        }

        public OperationResult<Paginated<Dtos.Wizard.TestTemplateHeaderDto>> GetTestTemplateHeaders(int authorId, int offset, int limit)
        {
            var tests = dbContext.TestTemplates
                .Where(x => x.AuthorId == authorId)
                .Include(x => x.TestItems)
                .Skip(offset)
                .Take(limit + 1)
                .Select(testMapper.MapToTestWizardHeaderDto);

            return new Paginated<Dtos.Wizard.TestTemplateHeaderDto>(limit, tests);
        }

        public OperationResult<List<QuestionAnswerDto>> GetUserAnswers(int userId, int testId)
        {
            var userAnswers = dbContext.UserAnswers
                .Where(x => x.ScheduledTestId == testId && x.UserId == userId)
                .OrderBy(x => x.QuestionId)
                .Select(questionMapper.MapToTestQuestionAnswerDto)
                .ToList();

            return userAnswers;
        }
    }
}
