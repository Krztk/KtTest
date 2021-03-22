using FluentAssertions;
using KtTest.Dtos.Test;
using KtTest.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.IntegrationTests.Tests
{
    [Collection(nameof(BaseFixture))]
    public class TestsControllerTests : IClassFixture<TestsControllerFixture>
    {
        private readonly BaseFixture fixture;
        private readonly TestsControllerFixture controllerFixture;

        public TestsControllerTests(BaseFixture fixture, TestsControllerFixture controllerFixture)
        {
            this.fixture = fixture;
            this.controllerFixture = controllerFixture;
        }

        [Fact]
        public async Task TeacherShouldGetTestResults()
        {
            ScheduledTest scheduledTest = controllerFixture.ScheduledTest;
            float maxScore = controllerFixture.TestMaxScore;
            string testName = controllerFixture.TestTemplate.Name;
            var studentIdUsername = fixture.OrganizationOwnerMembers[fixture.UserId]
                .Where(x => scheduledTest.UserTests.Any(y => y.UserId == x.Id))
                .ToDictionary(x => x.Id, x => x.UserName);

            List<UserTestResultDto> results = controllerFixture.StudentIdTestScore.Select(
                x => new UserTestResultDto
                {
                    UserId = x.Key,
                    UserScore = x.Value,
                    Status = TestStatus.Completed.ToString(),
                    Username = studentIdUsername[x.Key]
                }).ToList();


            var expectedDto = new GroupResultsDto
            {
                MaxTestScore = maxScore,
                Ended = true,
                TestId = scheduledTest.Id,
                TestName = testName,
                Results = results
            };

            var response = await fixture.RequestSender.GetAsync($"tests/{scheduledTest.Id}/results");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadAsStringAsync();
            var result = fixture.Deserialize<GroupResultsDto>(responseData);
            result.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public async Task StudentShouldGetTheyResult()
        {
            var scheduledTest = controllerFixture.ScheduledTest;
            var testTemplate = controllerFixture.TestTemplate;
            var testAuthorId = testTemplate.AuthorId;
            var student = fixture.OrganizationOwnerMembers[testAuthorId].First();
            var questions = controllerFixture.Questions;
            var QuestionIdUserAnswers = controllerFixture.UserAnswers
                .Where(x=>x.UserId == student.Id)
                .ToDictionary(x => x.QuestionId, x => x);

            var questionsWithResult = new List<QuestionWithResultDto>
            {
                new QuestionWithChoiceAnswerResultDto
                {
                    QuestionId = questions[0].Id,
                    Question = questions[0].Content,
                    Choices = questions[0]
                    .Answer.As<ChoiceAnswer>()
                    .Choices.Select((x, i) => new ChoiceDto
                    {
                        Value = x.Content,
                        Correct = x.Valid,
                        UserAnswer = (QuestionIdUserAnswers[questions[0].Id].As<ChoiceUserAnswer>().Value & (1 << i)) != 0
                    }).ToList()
                },
                new QuestionWithChoiceAnswerResultDto
                {
                    QuestionId = questions[1].Id,
                    Question = questions[1].Content,
                    Choices = questions[1]
                    .Answer.As<ChoiceAnswer>()
                    .Choices.Select((x, i) => new ChoiceDto
                    {
                        Value = x.Content,
                        Correct = x.Valid,
                        UserAnswer = (QuestionIdUserAnswers[questions[1].Id].As<ChoiceUserAnswer>().Value & (1 << i)) != 0
                    }).ToList()
                },
                new QuestionWithWrittenResultDto
                {
                    QuestionId = questions[2].Id,
                    Question = questions[2].Content,
                    CorrectAnswer = questions[2].Answer.As<WrittenAnswer>().Value,
                    UserAnswer = QuestionIdUserAnswers[questions[2].Id].As<WrittenUserAnswer>().Value
                },
            };

            var expectedDto = new TestResultsDto
            {
                Name = testTemplate.Name,
                QuestionsWithResult = questionsWithResult
            };

            var token = fixture.GenerateToken(student);
            var response = await fixture.RequestSender.GetAsync($"tests/{scheduledTest.Id}/result", token);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadAsStringAsync();
            var result = fixture.Deserialize<TestResultsDto>(responseData);
            result.Should().BeEquivalentTo(expectedDto);
        }
    }
}
