using FluentAssertions;
using KtTest.Dtos.Groups;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using Xunit;

namespace KtTest.Tests.MapperTests
{
    public class GroupMapperTests
    {
        [Fact]
        public void MapToGroupDto_Group_ValidDto()
        {
            //arrange
            var ownerId = 3;
            var groupId = 5;
            var groupName = "group #1";
            var group = new Group(groupId, groupName, ownerId);
            var expectedDto = new GroupDto
            {
                Id = groupId,
                Name = groupName
            };

            //act
            var mapper = new GroupServiceMapper();
            var dto = mapper.MapToGroupDto(group);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }
    }
}
