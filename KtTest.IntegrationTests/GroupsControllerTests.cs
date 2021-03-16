﻿using FluentAssertions;
using KtTest.Dtos.Groups;
using KtTest.Dtos.Organizations;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.IntegrationTests
{
    [Collection(nameof(BaseFixture))]
    public class GroupsControllerTests
    {
        private readonly BaseFixture fixture;

        public GroupsControllerTests(BaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task OrganizationOwnerShouldCreateGroup()
        {
            var dto = new CreateGroupDto
            {
                Name = "New group"
            };

            var json = fixture.Serialize(dto);
            var response = await fixture.client.PostAsync("groups", new StringContent(json, Encoding.UTF8, "application/json"));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadAsStringAsync();
            int groupId;
            int.TryParse(responseData, out groupId).Should().BeTrue();
            var group = await fixture.ExecuteDbContext(x => x.Groups.Include(x => x.GroupMembers).FirstOrDefaultAsync(x => x.Id == groupId));
            group.Name.Should().Be(dto.Name);
            group.GroupMembers.Should().OnlyContain(x => x.UserId == fixture.UserId);
        }

        [Fact]
        public async Task OwnerShouldAddUserToGroup()
        {
            var groupMember = fixture.OrganizationOwnerMembers[fixture.UserId][0];
            var userToInvite = fixture.OrganizationOwnerMembers[fixture.UserId][1];
            var group = new Group("TestGroup#1", fixture.UserId);
            group.GroupMembers.Add(new GroupMember { UserId = fixture.UserId });
            group.GroupMembers.Add(new GroupMember { UserId = groupMember.Id});
            await fixture.ExecuteDbContext(x => {
                x.Groups.Add(group);
                return x.SaveChangesAsync();
            });

            var dto = new AddMemberDto
            {
                UserId = userToInvite.Id
            };

            var json = fixture.Serialize(dto);
            int groupId = group.Id;
            var response = await fixture.client.PostAsync($"groups/{groupId}/members", new StringContent(json, Encoding.UTF8, "application/json"));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var groupFromDb = await fixture.ExecuteDbContext(x => x.Groups.Include(x => x.GroupMembers).FirstOrDefaultAsync(x => x.Id == groupId));
            groupFromDb.GroupMembers.Should().HaveCount(3);
            groupFromDb.GroupMembers.Select(x => x.UserId).Should().BeEquivalentTo(new List<int> { fixture.UserId, groupMember.Id, userToInvite.Id });
        }

        [Fact]
        public async Task ShouldGetGroupMembers()
        {
            var group = new Group("TestGroup#2", fixture.UserId);
            group.GroupMembers.Add(new GroupMember { UserId = fixture.UserId });
            var member = fixture.OrganizationOwnerMembers[fixture.UserId].First();
            group.GroupMembers.Add(new GroupMember { UserId = member.Id });

            await fixture.ExecuteDbContext(x => {
                x.Groups.Add(group);
                return x.SaveChangesAsync();
            });

            var groupId = group.Id;
            var response = await fixture.client.GetAsync($"groups/{groupId}/members");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseJson = await response.Content.ReadAsStringAsync();
            var members = fixture.Deserialize<List<UserDto>>(responseJson);
            var mapper = new OrganizationServiceMapper();
            var memberDtos = new List<AppUser> { fixture.TestUser, member }.Select(mapper.MapToUserDto);

            members.Should().BeEquivalentTo(memberDtos);
        }

        [Fact]
        public async Task ShouldGetAvailableUsers()
        {
            var group = new Group("TestGroup#3", fixture.UserId);
            group.GroupMembers.Add(new GroupMember { UserId = fixture.UserId });
            var member = fixture.OrganizationOwnerMembers[fixture.UserId].First();
            group.GroupMembers.Add(new GroupMember { UserId = member.Id });

            await fixture.ExecuteDbContext(x => {
                x.Groups.Add(group);
                return x.SaveChangesAsync();
            });

            var groupId = group.Id;
            var response = await fixture.client.GetAsync($"groups/{groupId}/available");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseJson = await response.Content.ReadAsStringAsync();
            var members = fixture.Deserialize<List<UserDto>>(responseJson);
            var mapper = new OrganizationServiceMapper();
            var membersInOrganizationButNotInGroup = fixture.OrganizationOwnerMembers[fixture.UserId]
                .Where(x=>x.Id != member.Id).Select(mapper.MapToUserDto);

            members.Should().BeEquivalentTo(membersInOrganizationButNotInGroup);
        }
    }
}