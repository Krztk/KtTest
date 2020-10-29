using KtTest.Dtos.Groups;
using KtTest.Dtos.Organizations;
using KtTest.Infrastructure.Data;
using KtTest.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Readers
{
    public class GroupReader
    {
        private readonly ReadOnlyAppDbContext dbContext;
        private readonly GroupServiceMapper groupMapper;
        private readonly OrganizationServiceMapper organizationMapper;

        public GroupReader(ReadOnlyAppDbContext dbContext, GroupServiceMapper groupMapper, OrganizationServiceMapper organizationMapper)
        {
            this.dbContext = dbContext;
            this.groupMapper = groupMapper;
            this.organizationMapper = organizationMapper;
        }

        public List<GroupDto> GetGroups(int organizationOwner)
        {
            return dbContext.Groups
                .Where(x => x.OwnerId == organizationOwner)
                .OrderByDescending(x => x.Id)
                .Select(groupMapper.MapToGroupDto)
                .ToList();
        }

        public List<UserDto> GetGroupMembers(int groupId)
        {
            return dbContext.GroupMembers
                .Include(x => x.User)
                .Where(x => x.GroupId == groupId)
                .Select(x => x.User)
                .Select(organizationMapper.MapToUserDto)
                .ToList();
        }
    }
}
