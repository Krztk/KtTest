using FluentAssertions;
using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using Xunit;

namespace KtTest.Tests.MapperTests
{
    public class CategoryMapperTests
    {
        [Fact]
        public void MapToCategoryDto_Category_ValidDto()
        {
            //arrange
            var authorId = 5;
            var categoryName = "category";
            var category = new Category(categoryName, authorId);
            int categoryId = 3;
            category.Id = categoryId;
            var expectedDto = new CategoryDto
            {
                Id = categoryId,
                Name = categoryName
            };
            //act
            var mapper = new CategoryServiceMapper();
            var dto = mapper.MapToCategoryDto(category);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }
    }
}
