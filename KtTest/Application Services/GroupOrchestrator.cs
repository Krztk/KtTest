using KtTest.Dtos.Groups;
using KtTest.Dtos.Organizations;
using KtTest.Extensions;
using KtTest.Readers;
using KtTest.Results;
using KtTest.Results.Errors;
using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class GroupOrchestrator
    {
        private readonly GroupService groupService;
        private readonly OrganizationService organizationService;
        private readonly GroupReader groupReader;
        private readonly IUserContext userContext;

        public GroupOrchestrator(GroupService groupService,
            OrganizationService organizationService,
            GroupReader groupReader,
            IUserContext userContext)
        {
            this.groupService = groupService;
            this.organizationService = organizationService;
            this.groupReader = groupReader;
            this.userContext = userContext;
        }

        public List<GroupHeaderDto> GetGroupHeaders()
        {
            if (userContext.IsOwner)
                return groupReader.GetGroupHeaders(userContext.UserId);

            return groupReader.GetGroupHeadersWithUser(userContext.UserId);
        }

        public async Task<OperationResult<GroupDto>> GetGroup(int groupId)
        {
            var userId = userContext.UserId;
            return await groupService.IsUserMemberOfGroup(userId, groupId)
                .Then(_ => groupReader.GetGroup(groupId));
        }

        public async Task<OperationResult<List<UserDto>>> GetGroupMembers(int groupId)
        {
            var userId = userContext.UserId;
            return await groupService.IsUserMemberOfGroup(userId, groupId)
                .Bind(_ => groupReader.GetGroupMembers(groupId));
        }

        public async Task<OperationResult<List<UserDto>>> GetAvailableUsers(int groupId)
        {
            var userId = userContext.UserId;
            return await groupService.IsUserMemberOfGroup(userId, groupId)
                .Then(_ => groupService.GetIdOfGroupOwner(groupId))
                .Bind(ownerId => groupReader.GetAvailableUsers(groupId, ownerId));
        }

        public async Task<OperationResult<int>> CreateGroup(CreateGroupDto createGroupDto)
        {
            return await groupService.CreateGroup(createGroupDto.Name);
        }

        public async Task<OperationResult<Unit>> AddMemberToGroup(int groupId, AddMemberDto addMemberDto)
        {
            int owner = userContext.UserId;
            int idOfAddedUser = addMemberDto.UserId;
            var isMember = await organizationService.IsUserMemberOfOrganization(owner, idOfAddedUser);
            
            if (!isMember)
            {
                return new BadRequestError();
            }

            return await groupService.AddUserToGroup(addMemberDto.UserId, groupId);
        }
    }
}
