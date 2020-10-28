using FluentValidation.AspNetCore;
using KtTest.Application_Services;
using KtTest.Configurations;
using KtTest.Dtos.Wizard;
using KtTest.Infrastructure.Data;
using KtTest.Infrastructure.Emails;
using KtTest.Infrastructure.Identity;
using KtTest.Infrastructure.JsonConverters;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using KtTest.Readers;
using KtTest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace KtTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(options =>
                options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection"))
                .EnableSensitiveDataLogging());

            services.AddDbContextPool<ReadOnlyAppDbContext>(options =>
                options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection"))
                .EnableSensitiveDataLogging());

            services.AddIdentity<AppUser, AppRole>(Options =>
            {
                Options.Password.RequireDigit = false;
                Options.Password.RequireLowercase = false;
                Options.Password.RequireUppercase = false;
                Options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = "http://localhost:5000",
                    ValidAudience = "http://localhost:5000",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Symmetric:Key"]))
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("OwnerOnly", x => x.RequireClaim("Owner"));
                options.AddPolicy("EmployeeOnly", x => x.RequireClaim("Employee"));
            });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new WizardQuestionDtoConverter());
                    options.JsonSerializerOptions.Converters.Add(new TestQuestionDtoConverter());
                    options.JsonSerializerOptions.Converters.Add(new QuestionAnswerDtoConverter());
                    options.JsonSerializerOptions.Converters.Add(new QuestionWithResultDtoConverter());
                })
                .AddFluentValidation(options =>
                {
                    options.RegisterValidatorsFromAssemblyContaining<CategoryDtoValidator>();
                });

            services.AddCors();
            services.AddHttpContextAccessor();
            services.AddScoped<CategoryOrchestrator, CategoryOrchestrator>();
            services.AddScoped<TestOrchestrator, TestOrchestrator>();
            services.AddScoped<QuestionOrchestrator, QuestionOrchestrator>();
            services.AddScoped<OrganizationOrchestrator, OrganizationOrchestrator>();
            services.AddScoped<GroupOrchestrator, GroupOrchestrator>();
            services.AddScoped<AuthOrchestrator, AuthOrchestrator>();
            services.AddScoped<QuestionService, QuestionService>();
            services.AddScoped<CategoryService, CategoryService>();
            services.AddScoped<TestService, TestService>();
            services.AddScoped<GroupService, GroupService>(); 
            services.AddScoped<OrganizationService, OrganizationService>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();
            services.AddScoped<IRegistrationCodeGenerator, RegistrationCodeGenerator>();
            services.AddScoped<IEmailSender, FakeEmailSender>();
            services.AddScoped<AuthService, AuthService>();

            services.AddSingleton<QuestionServiceMapper, QuestionServiceMapper>();
            services.AddSingleton<TestServiceMapper, TestServiceMapper>();
            services.AddSingleton<CategoryServiceMapper, CategoryServiceMapper>();
            services.AddSingleton<OrganizationServiceMapper, OrganizationServiceMapper>();

            services.AddTransient<QuestionReader, QuestionReader>();
            services.AddTransient<OrganizationReader, OrganizationReader>();

            services.AddOpenAPI();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "KtTest");
            });

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:3000");
                builder.AllowAnyHeader();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
