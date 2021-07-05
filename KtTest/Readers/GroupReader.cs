using KtTest.Dtos.Groups;
using KtTest.Dtos.Organizations;
using KtTest.Infrastructure.Data;
using KtTest.Infrastructure.Mappers;
using KtTest.Results;
using KtTest.Results.Errors;
using Microsoft.EntityFrameworkCore;
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

        public List<GroupHeaderDto> GetGroupHeaders(int organizationOwner)
        {
            return dbContext.Groups
                .Where(x => x.OwnerId == organizationOwner)
                .OrderByDescending(x => x.Id)
                .Select(groupMapper.MapToGroupHeader)
                .ToList();
        }

        public List<GroupHeaderDto> GetGroupHeadersWithUser(int userId)
        {
            return dbContext.GroupMembers
                .Include(x => x.Group)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.GroupId)
                .Select(x => groupMapper.MapToGroupHeader(x.Group))
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

        public List<UserDto> GetAvailableUsers(int groupId, int organizationOwner)
        {
            return dbContext.Users
                .Where(x => x.InvitedBy == organizationOwner && !x.GroupMembers.Any(x=>x.GroupId == groupId))
                .Select(organizationMapper.MapToUserDto)
                .ToList();
        }

        public OperationResult<GroupDto> GetGroup(int groupId)
        {
            var group = dbContext.Groups
                .Where(x => x.Id == groupId)
                .Include(x => x.GroupMembers)
                .ThenInclude(x => x.User)
                .Select(groupMapper.MapToGroup)
                .FirstOrDefault();

            if (group == null)
                return new DataNotFoundError();

            return group;
        }
    }
}
