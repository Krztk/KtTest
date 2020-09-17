using FluentAssertions;
using KtTest.Models;
using KtTest.Services;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.Tests.ServiceTests
{
    public class QuestionServiceTests : TestWithSqlite
    {
        [Fact]
        public async Task CreateQuestion_QuestionWithoutCategories_ReturnId()
        {
            //arrange
            var userId = 8;
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new QuestionService(dbContext, userContextMock.Object);

            var answer = new WrittenAnswer("4");
            var questionContent = "2 + 2 = ?";

            //act
            var result = await service.CreateQuestion(questionContent, answer, Enumerable.Empty<int>());

            //assert
            var persistedQuestion = dbContext.Questions.FirstOrDefault(x => x.Id == result);
            persistedQuestion.Content.Should().Be(questionContent);
            var numberOfCategories = dbContext.QuestionCategories.Where(x => x.QuestionId == result).Count();
            numberOfCategories.Should().Be(0);
        }

        [Fact]
        public async Task CreateQuestion_QuestionWithCategories_ReturnId()
        {
            //arrange
            var userId = 8;
            var categories = new List<Category> { new Category("Math", userId), new Category("Addition", userId) };
            dbContext.Categories.AddRange(categories);
            dbContext.SaveChanges();

            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new QuestionService(dbContext, userContextMock.Object);

            var answer = new WrittenAnswer("4");
            var questionContent = "2 + 2 = ?";
            var categoryIds = categories.Select(x => x.Id);

            //act
            var result = await service.CreateQuestion(questionContent, answer, categoryIds);

            //assert
            var persistedQuestion = dbContext.Questions.FirstOrDefault(x => x.Id == result);
            persistedQuestion.Content.Should().Be(questionContent);
            var questionCategories = dbContext.QuestionCategories
                .Where(x => x.QuestionId == result)
                .OrderBy(x=>x.CategoryId)
                .Select(x=>x.CategoryId).ToList();

            questionCategories.Count.Should().Be(2);
            questionCategories.Should().BeEquivalentTo(categoryIds);
        }

        [Fact]
        public void DoQuestionsExist_NotEveryQuestionIdExists_ReturnsFalse()
        {
            //arrange
            var userId = 11;
            var questionsInDb = new List<Question>
            {
                new Question("1st question", new WrittenAnswer("1st question's answer"), userId),
                new Question("2st question", new WrittenAnswer("2nd question's answer"), userId),
            };
            dbContext.Questions.AddRange(questionsInDb);
            dbContext.SaveChanges();
            questionsInDb.ForEach(x => x.Id.Should().NotBe(0));
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new QuestionService(dbContext, userContextMock.Object);

            //act
            var result = service.DoQuestionsExist(new List<int> { questionsInDb[0].Id, questionsInDb[1].Id, 5 });

            //assert
            result.Should().BeFalse();
        }
    }
}
