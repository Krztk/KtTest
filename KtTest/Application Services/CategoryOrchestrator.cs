using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Mappers;
using KtTest.Results;
using KtTest.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class CategoryOrchestrator
    {
        private readonly CategoryService categoryService;
        private readonly CategoryServiceMapper categoryMapper;

        public CategoryOrchestrator(CategoryService categoryService, CategoryServiceMapper categoryMapper)
        {
            this.categoryService = categoryService;
            this.categoryMapper = categoryMapper;
        }

        public async Task<OperationResult<int>> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            return await categoryService.CreateCategory(createCategoryDto.Name);
        }

        public async Task<List<CategoryDto>> GetCategories()
        {
            var categories = await categoryService.GetCategories();
            return categories
                .Select(categoryMapper.MapToCategoryDto)
                .ToList();
        }
    }
}
