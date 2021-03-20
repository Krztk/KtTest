using KtTest.IntegrationTests.Helpers;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.IntegrationTests
{
    [Collection(nameof(BaseFixture))]
    public class TestsControllerFixture : IAsyncLifetime
    {
        private readonly BaseFixture fixture;
        public List<Question> Questions { get; set; } = new List<Question>();
        public TestTemplate TestTemplate { get; set; }
        public List<UserAnswer> UserAnswers { get; } = new List<UserAnswer>();
        private int UserId { get; }
        public ScheduledTest ScheduledTest { get; private set; }
        public float TestMaxScore { get; private set; }
        public Dictionary<int, float> StudentIdTestScore { get; } = new Dictionary<int, float>();

        public TestsControllerFixture(BaseFixture fixture)
        {
            this.fixture = fixture;
            UserId = fixture.UserId;
        }

        private async Task AddPublishedTestWithAnswers()
        {
            var questionWithSingleValidChoice = Questions[0];
            var questionWith3ValidChoices = Questions[1];
            var questionWithWrittenAnswer = Questions[2];
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
            ScheduledTest = new ScheduledTest(TestTemplate.Id, testPublishDate, startDate, endDate, durationInMinutes, studentsIds);
            var userTestStartDate = startDate.AddMinutes(10);
            foreach (var userTest in ScheduledTest.UserTests)
            {
                userTest.SetStartDate(userTestStartDate);
                userTest.SetEndDate(userTestStartDate.AddMinutes(5));
            }

            await fixture.ExecuteDbContext(db =>
            {
                db.ScheduledTests.Add(ScheduledTest);
                return db.SaveChangesAsync();
            });

            int scheduledTestId = ScheduledTest.Id;

            //student1:
            UserAnswers.Add(
                UserAnswerGenerator.GenerateValidAnswer(questionWithSingleValidChoice, scheduledTestId, student1.Id));
            UserAnswers.Add(
                UserAnswerGenerator.GenerateValidAnswer(questionWith3ValidChoices, scheduledTestId, student1.Id));
            UserAnswers.Add(
                UserAnswerGenerator.GenerateValidAnswer(questionWithWrittenAnswer, scheduledTestId, student1.Id));

            //student2:
            UserAnswers.Add(
                UserAnswerGenerator.GenerateInvalidAnswer(questionWithSingleValidChoice, scheduledTestId, student2.Id));
            UserAnswers.Add(
                UserAnswerGenerator.GenerateUserAnswerWithNValidChoices(questionWith3ValidChoices, 1, scheduledTestId, student2.Id));
            UserAnswers.Add(
                UserAnswerGenerator.GenerateInvalidAnswer(questionWithWrittenAnswer, scheduledTestId, student2.Id));

            await fixture.ExecuteDbContext(db =>
            {
                db.UserAnswers.AddRange(UserAnswers);
                return db.SaveChangesAsync();
            });

            foreach (var userTest in ScheduledTest.UserTests)
            {
                float studentScore = 0f;
                foreach (var studentAnswer in UserAnswers.Where(x => x.UserId == userTest.UserId))
                {
                    var question = testTemplateQuestions[studentAnswer.QuestionId];
                    float questionScore = question.Answer.GetScore(studentAnswer);
                    studentScore += questionScore;
                }
                StudentIdTestScore.Add(userTest.UserId, studentScore);
            }

            TestMaxScore = testTemplateQuestions.Values.Select(x => x.Answer.MaxScore).Aggregate((x, y) => x + y);
        }

        private Task AddQuestions()
        {
            var choices = new List<Choice>
            {
                new Choice { Content = "32", Valid = false},
                new Choice { Content = "64", Valid = true},
                new Choice { Content = "81", Valid = false},

            };
            var answer = new ChoiceAnswer(choices, ChoiceAnswerType.SingleChoice, 2f);
            var question = new Question("What is the total number of squares on a chess board?", answer, UserId);
            Questions.Add(question);

            choices = new List<Choice>
            {
                new Choice { Content = "2", Valid = true},
                new Choice { Content = "3", Valid = true},
                new Choice { Content = "4", Valid = false},
                new Choice { Content = "5", Valid = true},
            };
            answer = new ChoiceAnswer(choices, ChoiceAnswerType.MultipleChoice, 3f);
            question = new Question("Select prime numbers", answer, UserId);
            Questions.Add(question);

            question = new Question("5 + 5 = ?", new WrittenAnswer("10", 1f), UserId);
            Questions.Add(question);

            return fixture.ExecuteDbContext(db =>
            {
                db.Questions.AddRange(Questions);
                return db.SaveChangesAsync();
            });
        }

        private Task AddTestTemplate()
        {
            TestTemplate = new TestTemplate("TestTemplate#1", UserId, Questions.Select(x => x.Id));
            return fixture.ExecuteDbContext(db =>
            {
                db.TestTemplates.Add(TestTemplate);
                return db.SaveChangesAsync();
            });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await AddQuestions();
            await AddTestTemplate();
            await AddPublishedTestWithAnswers();
        }
    }
}
