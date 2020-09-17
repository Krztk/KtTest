using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public class QuestionService
    {
        private readonly AppDbContext dbContext;
        private readonly IUserContext userContext;

        public QuestionService(AppDbContext dbContext, IUserContext userContext)
        {
            this.dbContext = dbContext;
            this.userContext = userContext;
        }

        public async Task<int> CreateQuestion(string content, Answer answer, IEnumerable<int> categoryIds)
        {
            var authorId = userContext.UserId;

            var question = new Question(content, answer, authorId);

            foreach (var categoryId in categoryIds)
                question.QuestionCategories.Add(new QuestionCategory { CategoryId = categoryId });

            dbContext.Questions.Add(question);
            await dbContext.SaveChangesAsync();
            return question.Id;
        }

        public async Task<PaginatedResult<Question>> GetQuestions(int offset, int limit)
        {
            int authorId = userContext.UserId;
            var questions = await dbContext.Questions
                .Where(x => x.AuthorId == authorId)
                .Include(x => x.Answer)
                    .ThenInclude(x => ((ChoiceAnswer)x).Choices)
                .Skip(offset)
                .Take(limit + 1)
                .ToListAsync();

            var result = new PaginatedResult<Question>();
            result.Data = new Paginated<Question>(limit, questions);
            return result;
        }

        public bool DoQuestionsExist(List<int> questionIds)
        {
            var authorId = userContext.UserId;
            var questionIdsFromDb = dbContext.Questions
                .Where(x => x.AuthorId == authorId)
                .Select(x => x.Id)
                .ToHashSet();

            return questionIds.All(x => questionIdsFromDb.Contains(x));
        }
    }
}
