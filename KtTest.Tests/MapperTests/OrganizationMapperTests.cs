using FluentAssertions;
using KtTest.Dtos.Organizations;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using System;
using Xunit;

namespace KtTest.Tests.MapperTests
{
    public class OrganizationMapperTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapToUserDto_AppUser_ValidDto(bool isTeacher)
        {
            //arrange
            var userId = 38;
            var username = "user1";
            var email = "test@example.com";
            var user = AppUser.CreateOrganizationMember(userId, email, username, isTeacher, 3);
            var expectedDto = new UserDto
            {
                Email = email,
                Id = userId,
                IsTeacher = isTeacher,
                Username = username
            };

            //act
            var mapper = new OrganizationServiceMapper();
            var dto = mapper.MapToUserDto(user);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapToInvitationDto_Invitation_ValidDto(bool isTeacher)
        {
            //arrange
            var email = "test@example.com";
            var code = "12F4567G912342i";
            var date = DateTime.UtcNow;
            int id = 8;
            var invitation = new Invitation(id, email, isTeacher, code, 2, date);
            var expectedDto = new InvitationDto
            {
                Id = id,
                Email = email,
                IsTeacher = isTeacher,
                Date = date
            };

            //act
            var mapper = new OrganizationServiceMapper();
            var dto = mapper.MapToInvitationDto(invitation);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }
    }
}
