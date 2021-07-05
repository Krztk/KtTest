using KtTest.Dtos.Groups;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Infrastructure.Mappers
{
    public class GroupServiceMapper
    {
        private readonly OrganizationServiceMapper organizationMapper;

        public GroupServiceMapper(OrganizationServiceMapper organizationMapper)
        {
            this.organizationMapper = organizationMapper;
        }

        public GroupHeaderDto MapToGroupHeader(Group group)
        {
            return new GroupHeaderDto
            {
                Id = group.Id,
                Name = group.Name
            };
        }

        public GroupDto MapToGroup(Group group)
        {
            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                GroupMembers = group.GroupMembers
                    .Select(member => organizationMapper.MapToUserDto(member.User))
                    .ToList()
            };
        }
    }
}
