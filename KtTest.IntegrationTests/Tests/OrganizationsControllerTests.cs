using FluentAssertions;
using KtTest.Dtos.Organizations;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.IntegrationTests.Tests
{
    [Collection(nameof(BaseFixture))]
    public class OrganizationsControllerTests
    {
        private readonly BaseFixture fixture;

        public OrganizationsControllerTests(BaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ShouldGetOrganizationMembers()
        {
            var organizationOwner = fixture.UserId;
            var response = await fixture.RequestSender.GetAsync("organizations/members");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseJson = await response.Content.ReadAsStringAsync();
            var members = fixture.Deserialize<List<UserDto>>(responseJson);
            var mapper = new OrganizationServiceMapper();
            var memberDtos = fixture.OrganizationOwnerMembers[organizationOwner].Select(mapper.MapToUserDto);

            members.Should().BeEquivalentTo(memberDtos);
        }

        [Theory]
        [InlineData("testTeacher@example.com", true)]
        [InlineData("testStudent@example.com", false)]
        public async Task ShouldInviteNewMember(string email, bool isTeacher)
        {
            var dto = new InviteUserDto
            {
                Email = email,
                IsTeacher = isTeacher,
            };

            var json = fixture.Serialize(dto);
            var response = await fixture.RequestSender.PostAsync($"organizations/invite", json);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var invitation = await fixture.ExecuteDbContext(db => db.Invitations.Where(x => x.Email == email).FirstOrDefaultAsync());
            invitation.InvitedBy.Should().Be(fixture.UserId);
            invitation.IsTeacher.Should().Be(isTeacher);
        }

        [Fact]
        public async Task ShouldGetInvitations()
        {
            var invitations = new List<Invitation>
            {
                new Invitation
                {
                    Code = Guid.NewGuid().ToString(),
                    Date = new DateTime(2021, 3, 3, 14, 0, 5, DateTimeKind.Utc),
                    Email = "testEmail1@example.com",
                    IsTeacher = false,
                    InvitedBy = fixture.UserId   
                },
                new Invitation
                {
                    Code = Guid.NewGuid().ToString(),
                    Date = new DateTime(2021, 3, 3, 15, 25, 3, DateTimeKind.Utc),
                    Email = "testEmail2@example.com",
                    IsTeacher = true,
                    InvitedBy = fixture.UserId
                },
            };
            
            await fixture.ExecuteDbContext(db =>
            {
                db.Invitations.AddRange(invitations);
                return db.SaveChangesAsync();
            });

            var mapper = new OrganizationServiceMapper();
            var expectedDtos = invitations.Select(mapper.MapToInvitationDto);

            var response = await fixture.RequestSender.GetAsync("organizations/invitations");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseJson = await response.Content.ReadAsStringAsync();
            var retunedInvitationDtos = fixture.Deserialize<InvitationDto[]>(responseJson);
            retunedInvitationDtos.Should().NotBeEmpty();
            foreach (var expectedDto in expectedDtos)
                retunedInvitationDtos.Should().ContainEquivalentOf(expectedDto);

        }
    }
}
