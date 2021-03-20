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
        private readonly HttpClient client;
        protected readonly IConfiguration configuration;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly Checkpoint checkpoint;
        public JsonSerializerOptions jsonSerializerOptions;
        public int UserId { get; private set; }

        public List<AppUser> OrganizationOwners { get; set; } = new List<AppUser>();
        public Dictionary<int, List<AppUser>> OrganizationOwnerMembers { get; set; } = new Dictionary<int, List<AppUser>>();
        public AppUser TestUser;
        public RequestSender RequestSender { get; private set; }
        public Dictionary<int, string> UserIdToken { get; } = new Dictionary<int, string>();

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

        public string GenerateToken(AppUser appUser)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
                return authService.GenerateToken(appUser);
            }
        }

        public async Task InitializeAsync()
        {
            await checkpoint.Reset(configuration.GetConnectionString("DefaultConnection"));
            await AddOrganizationOwners();
            await AddOrganizationMembers();
            TestUser = OrganizationOwners[0];
            UserId = TestUser.Id;
            var token = GenerateToken(TestUser);
            RequestSender = new RequestSender(client, token);
        }

        public Task DisposeAsync()
        {
            factory?.Dispose();
            return Task.CompletedTask;
        }
    }
}
