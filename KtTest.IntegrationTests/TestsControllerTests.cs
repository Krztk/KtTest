using FluentAssertions;
using KtTest.Dtos.Test;
using KtTest.IntegrationTests.Helpers;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.IntegrationTests
{
    [Collection(nameof(BaseFixture))]
    public class TestsControllerTests
    {
        private readonly BaseFixture fixture;

        public TestsControllerTests(BaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TeacherShouldGetTestResults()
        {
            var userAnswers = new List<UserAnswer>();
            var questionWithSingleValidChoice = fixture.Questions[0];
            var questionWith3ValidChoices = fixture.Questions[1];
            var questionWithWrittenAnswer = fixture.Questions[2];
            var testTemplateQuestions = new Dictionary<int, Question>
            {
                [questionWithSingleValidChoice.Id] = questionWithSingleValidChoice,
                [questionWith3ValidChoices.Id] = questionWith3ValidChoices,
                [questionWithWrittenAnswer.Id] = questionWithWrittenAnswer
            };

            var student1 = fixture.OrganizationOwnerMembers[fixture.UserId][0];
            var student2 = fixture.OrganizationOwnerMembers[fixture.UserId][1];
            var testPublishDate = new DateTime(2021, 1, 7, 6, 0, 0, DateTimeKind.Utc);
            var startDate = testPublishDate.AddDays(1);
            var endDate = startDate.AddHours(3);
            var durationInMinutes = 30;
            IEnumerable<int> studentsIds = new List<int> { student1.Id, student2.Id };
            var scheduledTest = new ScheduledTest(fixture.TestTemplate.Id, testPublishDate, startDate, endDate, durationInMinutes, studentsIds);
            var userTestStartDate = startDate.AddMinutes(10);
            foreach (var userTest in scheduledTest.UserTests)
            {
                userTest.SetStartDate(userTestStartDate);
                userTest.SetEndDate(userTestStartDate.AddMinutes(5));
            }

            await fixture.ExecuteDbContext(db =>
            {
                db.ScheduledTests.Add(scheduledTest);
                return db.SaveChangesAsync();
            });

            int scheduledTestId = scheduledTest.Id;

            //student1:
            userAnswers.Add(
                UserAnswerGenerator.GenerateValidAnswer(questionWithSingleValidChoice, scheduledTestId, student1.Id));
            userAnswers.Add(
                UserAnswerGenerator.GenerateValidAnswer(questionWith3ValidChoices, scheduledTestId, student1.Id));
            userAnswers.Add(
                UserAnswerGenerator.GenerateValidAnswer(questionWithWrittenAnswer, scheduledTestId, student1.Id));
            
            //student2:
            userAnswers.Add(
                UserAnswerGenerator.GenerateInvalidAnswer(questionWithSingleValidChoice, scheduledTestId, student2.Id));
            userAnswers.Add(
                UserAnswerGenerator.GenerateUserAnswerWithNValidChoices(questionWith3ValidChoices, 1, scheduledTestId, student2.Id));
            userAnswers.Add(
                UserAnswerGenerator.GenerateInvalidAnswer(questionWithWrittenAnswer, scheduledTestId, student2.Id));

            await fixture.ExecuteDbContext(db =>
            {
                db.UserAnswers.AddRange(userAnswers);
                return db.SaveChangesAsync();
            });

            float maxScore = fixture.Questions.Select(x => x.Answer.MaxScore).Aggregate((x, y) => x + y);
            float student1Score = maxScore;
            float student2Score = 0f;
            foreach (var student2Answer in userAnswers.Where(x => x.UserId == student2.Id))
            {
                var question = testTemplateQuestions[student2Answer.QuestionId];
                float questionScore = question.Answer.GetScore(student2Answer);
                student2Score += questionScore;
            }

            var expectedDto = new GroupResultsDto
            {
                MaxTestScore = maxScore,
                Ended = true,
                TestId = scheduledTest.Id,
                TestName = fixture.TestTemplate.Name,
                Results = new List<UserTestResultDto>
                {
                    new UserTestResultDto
                    {
                        UserId = student1.Id,
                        Status = TestStatus.Completed.ToString(),
                        Username = student1.UserName,
                        UserScore = maxScore
                    },
                    new UserTestResultDto
                    {
                        UserId = student2.Id,
                        Status = TestStatus.Completed.ToString(),
                        Username = student2.UserName,
                        UserScore = student2Score
                    },
                }
            };

            var response = await fixture.client.GetAsync($"tests/{scheduledTest.Id}/results");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadAsStringAsync();
            var result = fixture.Deserialize<GroupResultsDto>(responseData);
            result.Should().BeEquivalentTo(expectedDto);
        }
    }
}
