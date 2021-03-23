using KtTest.Exceptions.ServiceExceptions;
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

        public async Task<OperationResult> UpdateQuestion(int questionId, string content, Answer answer, IEnumerable<int> categoryIds)
        {
            var question = dbContext.Questions.Local.FirstOrDefault(x => x.Id == questionId);
            if (question == null)
                throw new ValueNotInTheCacheException("the question should already be in cache.");

            question.UpdateContent(content);
            if (categoryIds != null)
                question.ReplaceCategories(categoryIds);

            question.UpdateAnswer(answer);
            await dbContext.SaveChangesAsync();
            return new OperationResult();
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

        public async Task<bool> IsAuthorOfQuestion(int userId, int questionId)
        {
            var question = await dbContext.Questions
                    .Include(x => x.Answer)
                        .ThenInclude(x => ((ChoiceAnswer)x).Choices)
                    .Include(x => x.QuestionCategories)
                    .Where(x => x.Id == questionId)
                    .FirstOrDefaultAsync();

            if (question == null)
                return false;

            return question.AuthorId == userId;
        }
    }
}
