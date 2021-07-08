using FluentAssertions;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace KtTest.Tests.MapperTests
{
    public class TestMapperTests
    {
        private readonly TestServiceMapper mapper;

        public TestMapperTests()
        {
            mapper = new TestServiceMapper(new QuestionServiceMapper(), new DateTimeProvider());
        }

        [Fact]
        public void MapToTestHeaderDto_ScheduledTest_ValidDto()
        {
            //arrange
            var scheduledTestId = 16;
            var publishDate = new DateTime(2021, 3, 23, 14, 23, 7, DateTimeKind.Utc);
            var startDate = publishDate.AddHours(1);
            var endDate = startDate.AddHours(1);
            var duration = 30;
            var userIds = new int[] { 1, 2, 3 };
            var testTemplateId = 36;
            var testName = "MyTest#1";
            var questionIds = new int[] { 10, 11, 12 };
            var testTemplate = new TestTemplate(testTemplateId, testName, 1, questionIds);
            var scheduledTest = new ScheduledTest.ScheduledTestBuilder(duration, publishDate, startDate, endDate, userIds)
                .WithId(scheduledTestId)
                .WithTestTemplate(testTemplate)
                .Build();

            var expectedDto = new Dtos.Test.TestHeaderDto
            {
                Id = scheduledTestId,
                StartsAt = startDate,
                EndsAt = endDate,
                Name = testName
            };

            //act
            var dto = mapper.MapToTestHeaderDto(scheduledTest);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public void MapToTestWizardHeaderDto_TestTemplate_ValidDto()
        {
            //arrange
            var testTemplateId = 36;
            var publishDate = new DateTime(2021, 3, 23, 14, 23, 7, DateTimeKind.Utc);
            var testName = "MyTest#1";
            var questionIds = new int[] { 10, 11, 12 };
            var testTemplate = new TestTemplate(testTemplateId, testName, 1, questionIds);

            var expectedDto = new Dtos.Wizard.TestTemplateHeaderDto
            {
                Id = testTemplateId,
                Name = testName,
                NumberOfQuestions = questionIds.Length
            };

            //act
            var dto = mapper.MapToTestWizardHeaderDto(testTemplate);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public void MapToTestWizardDto_TestTemplate_ValidDto()
        {
            //arrange
            var testTemplateId = 36;
            var testName = "MyTest#1";
            var questionIds = new int[] { 10, 11, 12 };
            var questions = new List<Question>();
            foreach (var id in questionIds)
            {
                var question = new Question(id, "x", new WrittenAnswer("x", 1f), 1);
                questions.Add(question);
            }

            var testTemplate = new TestTemplate(testTemplateId, testName, 1, questions);

            var questionMapperMock = new Mock<IQuestionServiceMapper>();
            Expression<Func<IQuestionServiceMapper, Dtos.Wizard.QuestionDto>> setupExpression =
                x => x.MapToWizardQuestionDto(It.Is<Question>(q => questionIds.Contains(q.Id)));

            questionMapperMock
                .Setup(setupExpression)
                .Returns<Question>(x => new Dtos.Wizard.QuestionWithWrittenAnswerDto { Id = x.Id });

            //act
            var mapper = new TestServiceMapper(questionMapperMock.Object, new DateTimeProvider());
            var dto = mapper.MapToTestWizardDto(testTemplate);

            //assert
            questionMapperMock.Verify(setupExpression, Times.Exactly(questionIds.Length));
            questionMapperMock.VerifyNoOtherCalls();
            dto.Name.Should().Be(testName);
        }

        [Fact]
        public void MapToUserAnswer_WrittenUserAnswerDto_UserAnswer()
        {
            //arrange
            string value = "answer";
            int questionId = 1;
            int testId = 2;
            int userId = 3;
            var writtenAnswerDto = new Dtos.Test.WrittenAnswerDto
            {
                QuestionId = questionId,
                Text = value
            };
            UserAnswer expectedUserAnswer = new WrittenUserAnswer(value, testId, questionId, userId);

            //act
            var userAnswer = mapper.MapToUserAnswer(writtenAnswerDto, testId, userId);

            //assert
            userAnswer.Should().BeEquivalentTo(expectedUserAnswer);
        }
    }
}
