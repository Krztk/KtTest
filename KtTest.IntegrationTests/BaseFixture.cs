using KtTest.Infrastructure.Data;
using KtTest.IntegrationTests.Helpers;
using KtTest.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System;
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
        public int UserId { get; }
        private string _token = null;
        public string Token
        {
            get
            {
                if (_token != null)
                    return _token;

                using (var scope = scopeFactory.CreateScope())
                {
                    var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
                    _token = authService.GenerateToken(new Models.AppUser() { Id = UserId, InvitedBy = null, IsTeacher = true });
                    return _token;
                }
            }
        }

        public BaseFixture()
        {
            factory = new ApiWebApplicationFactory();
            configuration = factory.Services.GetRequiredService<IConfiguration>();
            scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();
            jsonSerializerOptions = JsonSerializerOptionsHelper.CreateOptions();
            UserId = 1;
            client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
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

        public Task InitializeAsync()
        {
            return checkpoint.Reset(configuration.GetConnectionString("DefaultConnection"));
        }

        public Task DisposeAsync()
        {
            factory?.Dispose();
            return Task.CompletedTask;
        }
    }
}
