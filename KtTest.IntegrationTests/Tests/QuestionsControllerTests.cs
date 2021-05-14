using FluentAssertions;
using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Data;
using KtTest.Infrastructure.Mappers;
using KtTest.IntegrationTests.ApiResponses;
using KtTest.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.IntegrationTests.Tests
{
    [Collection(nameof(BaseFixture))]
    public class QuestionsControllerTests
    {
        private readonly BaseFixture fixture;

        public QuestionsControllerTests(BaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ShouldCreateNewQuestion()
        {
            var questionDto = new QuestionWithWrittenAnswerDto
            {
                Answer = "answer",
                Question = "question",
                Score = 1.5f
            };
 
            var json = fixture.Serialize(questionDto);
            var response = await fixture.RequestSender.PostAsync($"questions", json);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadAsStringAsync();
            int questionId;
            int.TryParse(responseData, out questionId).Should().BeTrue();
            var created = await fixture.ExecuteDbContext(db => db.Questions.Include(x => x.Answer).FirstOrDefaultAsync(x => x.Id == questionId));
            created.Content.Should().Be(questionDto.Question);
            var answer = created.Answer as WrittenAnswer;
            answer.Value.Should().Be(questionDto.Answer);
            answer.MaxScore.Should().Be(questionDto.Score);
        }

        [Fact]
        public async Task ShouldNotCreateNewQuestionBecauseDtoIsNotValid()
        {
            var questionDtoWithEmptyAnswerField = new QuestionWithWrittenAnswerDto
            {
                Answer = "",
                Question = "question",
                Score = 1.5f
            };

            var json = fixture.Serialize(questionDtoWithEmptyAnswerField);
            var response = await fixture.RequestSender.PostAsync($"questions", json);
            var responseJson = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationError = fixture.Deserialize<ValidationErrorResponse>(responseJson);
            validationError.Errors.Should().ContainKey("Answer");
        }

        [Fact]
        public async Task ShouldNotCreateNewQuestionWithChoicesBecauseChoiceDtoIsNotValid()
        {
            var questionDtoWithChoiceContentTooLong = new QuestionWithChoiceAnswersDto
            {
                Choices = new List<ChoiceDto>
                {
                    new ChoiceDto
                    {
                        Content = "a",
                        Valid = false
                    },
                    new ChoiceDto
                    {
                        Content = new string('b', DataConstraints.Question.MaxAnswerLength + 1),
                        Valid = true
                    },
                },
                ChoiceAnswerType = ChoiceAnswerType.SingleChoice,
                AllValidChoicesRequired = true,
                Question = "question",
                Score = 1.5f
            };

            var json = fixture.Serialize(questionDtoWithChoiceContentTooLong);
            var response = await fixture.RequestSender.PostAsync($"questions", json);
            var responseJson = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var validationError = fixture.Deserialize<ValidationErrorResponse>(responseJson);
            validationError.Errors.Should().ContainKey("Choices[1].Content");
        }

        [Fact]
        public async Task ShouldReturnAllQuestions()
        {
            int authorId = fixture.UserId;
            var questions = new List<Question>();
            var choices = new List<Choice>()
            {
                new Choice { Valid = false, Content = "1957" },
                new Choice { Valid = true, Content = "1958" },
                new Choice { Valid = false, Content = "1959" },
                new Choice { Valid = false, Content = "1960" },
                new Choice { Valid = false, Content = "1961" },
                new Choice { Valid = false, Content = "1962" }
            };

            var choiceAnswer = new ChoiceAnswer(choices, ChoiceAnswerType.SingleChoice, 1f);
            questions.Add(new Question("When was Nasa founded?", choiceAnswer, authorId));

            choices = new List<Choice>()
            {
                new Choice { Valid = false, Content = "99 years" },
                new Choice { Valid = false, Content = "100 years" },
                new Choice { Valid = false, Content = "113 years" },
                new Choice { Valid = true, Content = "116 years" }
            };

            choiceAnswer = new ChoiceAnswer(choices, ChoiceAnswerType.SingleChoice, 1f);
            questions.Add(new Question("How many years did the 100 years war last?", choiceAnswer, authorId));

            var writtenAnswer = new WrittenAnswer("George Washington", 1f);
            questions.Add(new Question("Who was the first president of the United States?", writtenAnswer, authorId));

            await fixture.ExecuteDbContext(db =>
            {
                foreach (var question in questions)
                    db.Questions.Add(question);

                return db.SaveChangesAsync();
            });

            var mapper = new QuestionServiceMapper();
            var questionDtos = questions.Select(mapper.MapToWizardQuestionDto);

            int offset = 0;
            int limit = 10;
            var queryString = $"?Offset={offset}&Limit={limit}";
            var response = await fixture.RequestSender.GetAsync("questions" + queryString);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadAsStringAsync();
            var result = fixture.Deserialize<Paginated<QuestionDto>>(responseData);
            result.Data.Should().NotBeEmpty();
            foreach (var questionDto in questionDtos)
                result.Data.Should().ContainEquivalentOf(questionDto);
        }
    }
}
