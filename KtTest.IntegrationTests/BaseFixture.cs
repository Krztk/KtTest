using KtTest.Infrastructure.Data;
using KtTest.IntegrationTests.Helpers;
using KtTest.Models;
using KtTest.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.IntegrationTests
{
    [CollectionDefinition(nameof(BaseFixture))]
    public class BaseFixtureCollection : ICollectionFixture<BaseFixture> { }

    public class BaseFixture : IAsyncLifetime
    {
        protected readonly ApiWebApplicationFactory factory;
        public readonly HttpClient client;
        protected readonly IConfiguration configuration;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly Checkpoint checkpoint;
        public JsonSerializerOptions jsonSerializerOptions;
        public int UserId { get; private set; }

        public List<Models.AppUser> OrganizationOwners { get; set; } = new List<Models.AppUser>();
        public Dictionary<int, List<Models.AppUser>> OrganizationOwnerMembers { get; set; } = new Dictionary<int, List<Models.AppUser>>();
        public Models.AppUser TestUser;
        public List<Question> Questions { get; set; } = new List<Question>();
        public TestTemplate TestTemplate { get; set; }

        public BaseFixture()
        {
            factory = new ApiWebApplicationFactory();
            configuration = factory.Services.GetRequiredService<IConfiguration>();
            scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
            jsonSerializerOptions = JsonSerializerOptionsHelper.CreateOptions();
            client = factory.CreateClient();
            checkpoint = new Checkpoint();
        }

        public string Serialize<T>(T entity)
        {
            return JsonSerializer.Serialize<T>(entity, jsonSerializerOptions);
        }

        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
        }

        public async Task<T> Find<T>(int id)
            where T : class
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await dbContext.Set<T>().FindAsync(id).AsTask();
        }

        public async Task<T> ExecuteScope<T>(Func<IServiceProvider, Task<T>> action)
        {
            using var scope = scopeFactory.CreateScope();
            return await action(scope.ServiceProvider);
        }

        public async Task ExecuteScope(Func<IServiceProvider, Task> action)
        {
            using var scope = scopeFactory.CreateScope();
            await action(scope.ServiceProvider);
        }

        public Task ExecuteDbContext(Func<AppDbContext, Task> action)
            => ExecuteScope(sp => action(sp.GetService<AppDbContext>()));

        public Task<T> ExecuteDbContext<T>(Func<AppDbContext, Task<T>> action)
            => ExecuteScope(sp => action(sp.GetService<AppDbContext>()));

        public Task AddUser(Models.AppUser user)
        {
            Func<UserManager<Models.AppUser>, Task> action = async x =>
            {
                var result = await x.CreateAsync(user, "password");
                if (!result.Succeeded)
                    throw new Exception(string.Join(",", result.Errors));
            };
            return ExecuteScope(sp => action(sp.GetService<UserManager<Models.AppUser>>()));
        }

        private async Task AddOrganizationOwners()
        {
            for (int i = 1; i <= 4; i++)
                OrganizationOwners.Add(new Models.AppUser { UserName = $"Owner{i}", IsTeacher = true, InvitedBy = null });

            foreach (var owner in OrganizationOwners)
            {
                await AddUser(owner);
            }
        }

        private async Task AddOrganizationMembers()
        {
            foreach (var owner in OrganizationOwners)
            {
                if (owner.Id == 0)
                    throw new Exception("Wrong user ID");

                for (int i = 1; i <= 3; i++)
                {
                    var member = new Models.AppUser { UserName = $"user{i}_invitedby_{owner.Id}", IsTeacher = false, InvitedBy = owner.Id };
                    if (OrganizationOwnerMembers.ContainsKey(owner.Id))
                    {
                        OrganizationOwnerMembers[owner.Id].Add(member);
                    }
                    else
                        OrganizationOwnerMembers.Add(owner.Id, new List<Models.AppUser> { member });
                }
            }

            foreach (var member in OrganizationOwnerMembers.Values.SelectMany(x => x))
            {
                await AddUser(member);
            }
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

            return ExecuteDbContext(db =>
            {
                db.Questions.AddRange(Questions);
                return db.SaveChangesAsync();
            });
        }
        private Task AddTestTemplate()
        {
            TestTemplate = new TestTemplate("TestTemplate#1", UserId, Questions.Select(x => x.Id));
            return ExecuteDbContext(db =>
            {
                db.TestTemplates.Add(TestTemplate);
                return db.SaveChangesAsync();
            });
        }

        public async Task InitializeAsync()
        {
            await checkpoint.Reset(configuration.GetConnectionString("DefaultConnection"));
            await AddOrganizationOwners();
            await AddOrganizationMembers();
            TestUser = OrganizationOwners[0];
            UserId = TestUser.Id;
            await AddQuestions();
            await AddTestTemplate();
            var token = String.Empty;
            using (var scope = scopeFactory.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
                token = authService.GenerateToken(TestUser);
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public Task DisposeAsync()
        {
            factory?.Dispose();
            return Task.CompletedTask;
        }
    }
}
