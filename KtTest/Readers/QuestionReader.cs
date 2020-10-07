﻿using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Readers
{
    public class QuestionReader
    {
        private readonly ReadOnlyAppDbContext dbContext;

        public QuestionReader(ReadOnlyAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<PaginatedResult<QuestionHeaderDto>> GetQuestionHeaders(int authorId, int offset, int limit)
        {
            var questionTests = await dbContext.Questions.Where(x => x.AuthorId == authorId).Take(limit + 1).Skip(offset)
                                                   .GroupJoin(dbContext.TestItems,
                                                           x => x.Id,
                                                           y => y.QuestionId,
                                                           (x, y) => new { x.Id, x.Content, TestItems = y })
                                                   .SelectMany(x => x.TestItems.DefaultIfEmpty(),
                                                               (x, y) => new { x.Id, x.Content, TestItem = y })
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
                        Content = question.Content,
                        NumberOfTimesUsedInTests = question.TestItem == null ? 0 : 1
                    };
                    idHeaderDtos.Add(question.Id, questionHeaderDto);
                }
            }

            var result = new PaginatedResult<QuestionHeaderDto>();
            result.Data = new Paginated<QuestionHeaderDto>(limit, idHeaderDtos.Values);
            return result;
        }
    }
}