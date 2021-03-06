﻿using FluentAssertions;
using KtTest.Dtos.Groups;
using KtTest.Infrastructure.Mappers;
using KtTest.Models;
using Xunit;

namespace KtTest.Tests.MapperTests
{
    public class GroupMapperTests
    {
        [Fact]
        public void MapToGroupHeader_Group_ValidDto()
        {
            //arrange
            var ownerId = 3;
            var groupId = 5;
            var groupName = "group #1";
            var group = new Group(groupId, groupName, ownerId);
            var expectedDto = new GroupHeaderDto
            {
                Id = groupId,
                Name = groupName
            };

            //act
            var mapper = new GroupServiceMapper(new OrganizationServiceMapper());
            var dto = mapper.MapToGroupHeader(group);

            //assert
            dto.Should().BeEquivalentTo(expectedDto);
        }
    }
}
