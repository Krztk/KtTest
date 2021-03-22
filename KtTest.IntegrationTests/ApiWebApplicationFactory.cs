using KtTest.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KtTest.IntegrationTests
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                var integrationConfig = new ConfigurationBuilder()
                    .AddJsonFile("integrationSettings.json")
                    .Build();

                config.AddConfiguration(integrationConfig);
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IDateTimeProvider, IntegrationTestsDateTimeProvider>();
            });
        }
    }
}
