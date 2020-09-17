using KtTest.Dtos.Wizard;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Infrastructure.Mappers
{
    public class CategoryServiceMapper
    {
        public CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}
