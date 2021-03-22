using FluentAssertions;
using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.IntegrationTests.Tests
{
    [Collection(nameof(BaseFixture))]
    public class CategoriesControllerTests
    {
        private readonly BaseFixture fixture;

        public CategoriesControllerTests(BaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ShouldCreateCategory()
        {
            var dto = new CreateCategoryDto { Name = "Math" };
            var json = fixture.Serialize(dto);
            var response = await fixture.RequestSender.PostAsync("categories", json);
            var responseJson = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadAsStringAsync();
            int categoryId;
            int.TryParse(responseData, out categoryId).Should().BeTrue();
            var category = await fixture.Find<Category>(categoryId);
            category.Name.Should().Be(dto.Name);
            category.UserId.Should().Be(fixture.UserId);
        }

        [Fact]
        public async Task ShouldGetAllCategories()
        {
            var categories = new List<Category>
            {
                new Category("Test Category #1", fixture.UserId),
                new Category("Test Category #2", fixture.UserId),
            };

            await fixture.ExecuteDbContext(db =>
            {
                db.Categories.AddRange(categories);
                return db.SaveChangesAsync();
            });

            var mapper = new CategoryServiceMapper();
            var categoryDtos = categories.Select(mapper.MapToCategoryDto).ToArray();

            var response = await fixture.RequestSender.GetAsync("categories");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseJson = await response.Content.ReadAsStringAsync();
            var returnedCategories = fixture.Deserialize<CategoryDto[]>(responseJson);
            returnedCategories.Should().NotBeEmpty();
            foreach (var categoryDto in categoryDtos)
                returnedCategories.Should().ContainEquivalentOf(categoryDto);
        }
    }
}
