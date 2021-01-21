using FluentAssertions;
using KtTest.Exceptions.ServiceExceptions;
using KtTest.Models;
using KtTest.Results;
using KtTest.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            float maxScore = 3f;
            var answer = new WrittenAnswer("4", maxScore);
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
            float maxScore = 3f;
            var answer = new WrittenAnswer("4", maxScore);
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
            float maxScore = 3f;
            var questionsInDb = new List<Question>
            {
                new Question("1st question", new WrittenAnswer("1st question's answer", maxScore), userId),
                new Question("2st question", new WrittenAnswer("2nd question's answer", maxScore), userId),
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

        [Fact]
        public async Task UpdateQuestion_ChangeWrittenAnswerToChoiceAnswer_UpdatesQuestion()
        {
            //arrange
            var userId = 8;
            float maxScore = 6f;
            var answer = new WrittenAnswer("Answer to edit", maxScore);
            var question = new Question("Question to edit", answer, userId);
            dbContext.Questions.Add(question);
            dbContext.SaveChanges();
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new QuestionService(dbContext, userContextMock.Object);
            var choices = new List<Choice>
            {
                new Choice
                {
                    Content = "First choice",
                    Valid = false
                },
                new  Choice
                {
                    Content = "Second choice",
                    Valid = true
                },
            };
            float newMaxScore = 10f;
            var newAnswer = new ChoiceAnswer(choices, ChoiceAnswerType.SingleChoice, newMaxScore);

            //act
            var result = await service.UpdateQuestion(question.Id, "New content", newAnswer, null);

            //assert
            result.Succeeded.Should().BeTrue();
            var persistedAnswer = dbContext.Answers.Include(x=>((ChoiceAnswer)x).Choices).First(x => x.QuestionId == question.Id);
            var choiceAnswer = persistedAnswer as ChoiceAnswer;
            choiceAnswer.Should().NotBeNull();
            choiceAnswer.NumericValue.Should().Be(1);
            choiceAnswer.Choices.Should().BeEquivalentTo(choices);
            choiceAnswer.MaxScore.Should().Be(newMaxScore);
        }

        [Fact]
        public async Task UpdateQuestion_MethodExpectsQuestionInTheCache_ThrowsException()
        {
            //arrange
            var userId = 8;
            float maxScore = 6f;
            var answer = new WrittenAnswer("Answer to edit", maxScore);
            var question = new Question("Question to edit", answer, userId);
            dbContext.Questions.Add(question);
            dbContext.SaveChanges();
            dbContext.Questions.Local.Clear();
            var userContextMock = new Mock<IUserContext>();
            userContextMock.Setup(x => x.UserId).Returns(userId);
            var service = new QuestionService(dbContext, userContextMock.Object);
            var newAnswer = new WrittenAnswer("New answer", maxScore);

            //act
            Func<Task<OperationResult>> codeUnderTest = async () => await service.UpdateQuestion(question.Id, "New content", newAnswer, null);

            //assert
            await codeUnderTest.Should().ThrowExactlyAsync<ValueNotInTheCacheException>();
        }
    }
}
