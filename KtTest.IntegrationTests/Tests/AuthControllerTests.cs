using FluentAssertions;
using KtTest.Dtos.Auth;
using KtTest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.IntegrationTests.Tests
{
    [Collection(nameof(BaseFixture))]
    public class AuthControllerTests
    {
        private readonly BaseFixture fixture;
        public AuthControllerTests(BaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ShouldRegisterOrganizationOwner()
        {
            var email = "testEmail@test.com";
            var username = "username1";
            var normalizedEmail = email.ToUpper();

            var dto = new RegisterDto { Email = email, Username = username, Password = "password1"};
            var json = fixture.Serialize(dto);
            var response = await fixture.RequestSender.PostAsync("auth/register", json);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var user = await fixture.ExecuteDbContext(dbContext => dbContext.Users.FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail));
            user.Should().NotBeNull();
            user.Email.Should().Be(email);
            user.UserName.Should().Be(username);
        }
    }
}
