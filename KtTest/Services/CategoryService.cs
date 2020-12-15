using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
using KtTest.Results.Errors;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public class CategoryService
    {
        private readonly AppDbContext dbContext;
        private readonly IUserContext userContext;

        public CategoryService(AppDbContext dbContext, IUserContext userContext)
        {
            this.dbContext = dbContext;
            this.userContext = userContext;
        }

        public async Task<List<Category>> GetCategories()
        {
            var userId = userContext.UserId;
            var categories = await dbContext.Categories.Where(x => x.UserId == userId).ToListAsync();
            return categories;
        }

        public async Task<OperationResult<int>> CreateCategory(string name)
        {
            var authorId = userContext.UserId;
            var category = new Category(name, authorId);

            var alreadyExists = await dbContext.Categories.Where(x => x.Name == name).CountAsync() > 0;
            if (alreadyExists)
            {
                return new BadRequestError("Category with that name already exists");
            }

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
            return category.Id;
        }

        public bool DoCategoriesExist(IEnumerable<int> categoryIds)
        {
            var categoryIdsFromDb = dbContext.Categories
                .Where(x => x.UserId == userContext.UserId)
                .Select(x => x.Id)
                .ToHashSet();

            return categoryIds.All(x => categoryIdsFromDb.Contains(x));
        }
    }
}
