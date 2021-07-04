using KtTest.Dtos.Groups;
using KtTest.Dtos.Organizations;
using KtTest.Readers;
using KtTest.Results;
using KtTest.Results.Errors;
using KtTest.Services;
using Microsoft.AspNetCore.Authorization;
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
            IUserContext userContext,
            IAuthorizationService authorizationService)
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

        public async Task<OperationResult<List<UserDto>>> GetGroupMembers(int groupId)
        {
            var userId = userContext.UserId;
            var getIdOfGroupOwnerResult = await organizationService.GetIdOfGroupOwner(groupId);
            if (!getIdOfGroupOwnerResult.Succeeded)
                return getIdOfGroupOwnerResult.Error;

            var isMember = await organizationService
                .IsUserMemberOfOrganization(getIdOfGroupOwnerResult.Data, userId);

            if (!isMember)
            {
                return new BadRequestError();
            }

            return groupReader.GetGroupMembers(groupId);
        }

        public async Task<OperationResult<List<UserDto>>> GetAvailableUsers(int groupId)
        {
            var userId = userContext.UserId;
            var getIdOfGroupOwnerResult = await organizationService.GetIdOfGroupOwner(groupId);
            if (!getIdOfGroupOwnerResult.Succeeded)
                return getIdOfGroupOwnerResult.Error;

            var isMember = await organizationService
                .IsUserMemberOfOrganization(getIdOfGroupOwnerResult.Data, userId);

            if (!isMember)
            {
                return new BadRequestError();
            }

            return groupReader.GetAvailableUsers(groupId, getIdOfGroupOwnerResult.Data);
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
