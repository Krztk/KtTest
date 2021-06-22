using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Data;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Results;
using KtTest.Results.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Readers
{
    public class QuestionReader
    {
        private readonly ReadOnlyAppDbContext dbContext;
        private readonly IQuestionServiceMapper questionMapper;

        public QuestionReader(ReadOnlyAppDbContext dbContext, IQuestionServiceMapper questionMapper)
        {
            this.dbContext = dbContext;
            this.questionMapper = questionMapper;
        }

        public async Task<OperationResult<Paginated<QuestionHeaderDto>>> GetQuestionHeaders(int authorId, int offset, int limit)
        {
            var questionTests = await dbContext
                .Questions
                .FromSqlInterpolated($"SELECT Id, Content from Questions WHERE AuthorId = {authorId} ORDER BY Id DESC OFFSET {offset} ROWS FETCH NEXT {limit + 1} ROWS ONLY")
                .Include(x => x.Answer)
                .GroupJoin(dbContext.TestItems,
                    x => x.Id,
                    y => y.QuestionId,
                    (x, y) => new { x.Id, x.Content, x.Answer, TestItem = y })
                .SelectMany(x => x.TestItem.DefaultIfEmpty(),
                    (x, y) => new { x.Id, x.Content, x.Answer, TestItem = y })
                .ToArrayAsync();

            var idHeaderDtos = new Dictionary<int, QuestionHeaderDto>();
            foreach (var question in questionTests)
            {
                if (idHeaderDtos.ContainsKey(question.Id))
                {
                    if (question.TestItem != null)
                        idHeaderDtos[question.Id].NumberOfTimesUsedInTests++;
                }
                else
                {
                    var questionHeaderDto = new QuestionHeaderDto
                    {
                        Id = question.Id,
                        Content = question.Content,
                        NumberOfTimesUsedInTests = question.TestItem == null ? 0 : 1,
                        Type = question.Answer switch
                        {
                            WrittenAnswer writtenAnswer => "Written",
                            ChoiceAnswer choiceAnswer => choiceAnswer.ChoiceAnswerType == ChoiceAnswerType.SingleChoice
                                ? "Single choice"
                                : "Multiple choice",
                            _ => throw new NotImplementedException()
                        }
                    };
                    idHeaderDtos.Add(question.Id, questionHeaderDto);
                }
            }

            return new Paginated<QuestionHeaderDto>(limit, idHeaderDtos.Values);
        }

        public OperationResult<Paginated<QuestionDto>> GetQuestions(int authorId, int offset, int limit)
        {
            var questions = dbContext.Questions
                .Where(x => x.AuthorId == authorId)
                .Include(x => x.Answer)
                    .ThenInclude(x => ((ChoiceAnswer)x).Choices)
                .Skip(offset)
                .Take(limit + 1)
                .Select(questionMapper.MapToWizardQuestionDto)
                .ToArray();

            return new Paginated<QuestionDto>(limit, questions);
        }

        public async Task<OperationResult<QuestionDto>> GetQuestion(int authorId, int questionId)
        {
            var question = await dbContext.Questions
                .Where(x => x.Id == questionId)
                .Include(x => x.Answer)
                    .ThenInclude(x => ((ChoiceAnswer)x).Choices)
                .Select(x => new { QuestionDto = questionMapper.MapToWizardQuestionDto(x), x.AuthorId })
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return new DataNotFoundError();
            }

            if (authorId != question.AuthorId)
            {
                return new AuthorizationError();
            }

            return question.QuestionDto;
        }
    }
}