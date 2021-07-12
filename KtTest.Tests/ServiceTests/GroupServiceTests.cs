using FluentAssertions;
using KtTest.Models;
using KtTest.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace KtTest.Tests.ServiceTests
{
    public class GroupServiceTests : TestWithSqlite
    {
        private readonly int userId;
        readonly IUserContext userContext;

        public GroupServiceTests()
        {
            var user = AppUser.CreateOrganizationOwner("owner@test.com", "user1");
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
            var userContextMock = new Mock<IUserContext>();
            userId = user.Id;
            userContextMock.Setup(x => x.UserId).Returns(userId);
            userContext = userContextMock.Object;
        }

        [Fact]
        public async Task CreateGroup_ValidData_GroupCreated()
        {
            //arrange
            var groupName = "Group name # 1";
            var groupService = new GroupService(dbContext, userContext);

            //act
            var result = await groupService.CreateGroup(groupName);

            //assert
            result.Succeeded.Should().BeTrue();
            var groupId = result.Data;
            var group = dbContext.Groups.Include(x => x.GroupMembers).FirstOrDefault(x => x.Id == groupId);
            group.Name.Should().Be(groupName);
            group.GroupMembers.Should().HaveCount(1);
            group.GroupMembers.Should().OnlyContain(x => x.UserId == userId);
        }

        [Fact]
        public async Task GetStudentsInGroup_GroupWithOnlyTeacher_ReturnsOperationResultWithError()
        {
            //arrange
            var group = new Group("g1", userId);
            InsertData(group);
            var groupService = new GroupService(dbContext, userContext);

            //act
            var result = await groupService.GetStudentsInGroup(group.Id);

            //assert
            result.Succeeded.Should().BeFalse();
        }

        [Fact]
        public async Task GetStudentsInGroup_GroupWithStudentsAndTeachers_ReturnsOnlyStudents()
        {
            //arrange
            var group = new Group("g1", userId);
            var organizationMembers = new List<AppUser>
            {
                AppUser.CreateOrganizationMember("student1@test.com", "student1", false, userId),
                AppUser.CreateOrganizationMember("student2@test.com", "student2", false, userId),
                AppUser.CreateOrganizationMember("student3@test.com", "student3", false, userId),
                AppUser.CreateOrganizationMember("teacher1@test.com", "teacher1", true, userId),
            };

            dbContext.Users.AddRange(organizationMembers);
            await dbContext.SaveChangesAsync();

            foreach (var organizationMember in organizationMembers)
                group.AddMember(organizationMember.Id);

            InsertData(group);
            var groupService = new GroupService(dbContext, userContext);

            //act
            var result = await groupService.GetStudentsInGroup(group.Id);

            //assert
            result.Succeeded.Should().BeTrue();
            result.Data.Should().HaveCount(3);
            var expectedStudentsIds = organizationMembers.Where(x => !x.IsTeacher).Select(x => x.Id);
            result.Data.Select(x => x.Id).Should().BeEquivalentTo(expectedStudentsIds);
        }

        [Fact]
        public async Task IsUserMemberOfGroup_IsMember_ReturnsSuccessfulResult()
        {
            //arrange
            var group = new Group("group", userId);
            InsertData(group);
            var groupService = new GroupService(dbContext, userContext);

            //act
            var result = await groupService.IsUserMemberOfGroup(userId, group.Id);

            //assert
            result.Succeeded.Should().BeTrue();
        }


        [Fact]
        public async Task IsUserMemberOfGroup_IsntMember_ReturnsResultWithError()
        {
            //arrange
            int groupOwner = 1;
            var group = new Group("group", groupOwner);
            int userThatIsntMemberOfGroup = 2;
            InsertData(group);
            var groupService = new GroupService(dbContext, userContext);

            //act
            var result = await groupService.IsUserMemberOfGroup(userThatIsntMemberOfGroup, group.Id);

            //assert
            result.Succeeded.Should().BeFalse();
        }
    }
}
