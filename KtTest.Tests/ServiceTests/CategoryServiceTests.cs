using FluentAssertions;
using KtTest.Models;
using KtTest.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.Tests.ServiceTests
{
    public class CategoryServiceTests : TestWithSqlite
    {
        public CategoryServiceTests()
        {
        }

        [Fact]
        public void DoCategoriesExist_NoUsersCategories_ReturnsFalse()
        {
            //arrange
            var userId = 11;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new CategoryService(dbContext, userContextMock.Object);

            //act
            var result = service.DoCategoriesExist(new List<int> { 1, 2, 3 });

            //assert
            result.Should().BeFalse();
        }

        [Fact]
        public void DoCategoriesExist_OneCategoryIdDoesntExist_ReturnsFalse()
        {
            //arrange
            var userId = 11;
            var categoriesInDb = new List<Category>
            {
                new Category("A", userId),
                new Category("B", userId)
            };
            dbContext.Categories.AddRange(categoriesInDb);
            dbContext.SaveChanges();
            categoriesInDb.ForEach(x => x.Id.Should().NotBe(0));
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new CategoryService(dbContext, userContextMock.Object);

            //act
            var result = service.DoCategoriesExist(new List<int> { categoriesInDb[0].Id, categoriesInDb[1].Id, 5 });

            //assert
            result.Should().BeFalse();
        }

        [Fact]
        public void DoCategoriesExist_AllCategoryIdsAreValid_ReturnsTrue()
        {
            //arrange
            var userId = 11;
            var categoriesInDb = new List<Category>
            {
                new Category("A", userId),
                new Category("B", userId)
            };
            dbContext.Categories.AddRange(categoriesInDb);
            dbContext.SaveChanges();
            categoriesInDb.ForEach(x => x.Id.Should().NotBe(0));
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new CategoryService(dbContext, userContextMock.Object);

            //act
            var result = service.DoCategoriesExist(new List<int> { categoriesInDb[0].Id, categoriesInDb[1].Id });

            //assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CreateCategory_ValidData_ReturnsSuccessResultWithId()
        {
            //arrange
            var userId = 8;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new CategoryService(dbContext, userContextMock.Object);
            var categoryName = "New category";

            //act
            var result = await service.CreateCategory(categoryName);

            //assert
            result.Succeeded.Should().BeTrue();
            var createdCategory = dbContext.Categories.Single(x => x.Name == categoryName);
            result.Data.Should().Be(createdCategory.Id);
        }

        [Fact]
        public async Task CreateCategory_CategoryWithTheSameNameAlreadyExists_ReturnsFailureResult()
        {
            //arrange
            var userId = 8;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new CategoryService(dbContext, userContextMock.Object);
            var categoryName = "New category";
            dbContext.Categories.Add(new Category(categoryName, userId));
            await dbContext.SaveChangesAsync();

            //act
            var result = await service.CreateCategory(categoryName);

            //assert
            result.Succeeded.Should().BeFalse();
        }
    }
}
