using KtTest.Dtos.Groups;
using KtTest.Dtos.Organizations;
using KtTest.Readers;
using KtTest.Results;
using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class GroupOrchestrator
    {
        private readonly GroupService groupService;
        private readonly OrganizationService organizationService;
        private readonly GroupReader groupReader;
        private readonly IUserContext userContext;

        public GroupOrchestrator(GroupService groupService, OrganizationService organizationService, GroupReader groupReader, IUserContext userContext)
        {
            this.groupService = groupService;
            this.organizationService = organizationService;
            this.groupReader = groupReader;
            this.userContext = userContext;
        }

        public List<GroupDto> GetGroups()
        {
            return groupReader.GetGroups(userContext.UserId);
        }

        public async Task<OperationResult<List<UserDto>>> GetGroupMembers(int groupId)
        {
            var userId = userContext.UserId;
            var getIdOfGroupOwnerResult = await organizationService.GetIdOfGroupOwner(groupId);
            if (!getIdOfGroupOwnerResult.Succeeded)
                return getIdOfGroupOwnerResult.MapResult<List<UserDto>>();

            var isMember = await organizationService
                .IsUserMemberOfOrganization(getIdOfGroupOwnerResult.Data, userId);

            var result = new OperationResult<List<UserDto>>();
            if (!isMember)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            result.Data = groupReader.GetGroupMembers(groupId);
            return result;
        }

        public async Task<OperationResult<int>> CreateGroup(CreateGroupDto createGroupDto)
        {
            return await groupService.CreateGroup(createGroupDto.Name);
        }

        public async Task<OperationResult> AddMemberToGroup(int groupId, AddMemberDto addMemberDto)
        {
            int owner = userContext.UserId;
            int idOfAddedUser = addMemberDto.UserId;
            var isMember = await organizationService.IsUserMemberOfOrganization(owner, idOfAddedUser);
            
            if (!isMember)
            {
                var result = new OperationResult();
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            return await groupService.AddUserToGroup(addMemberDto.UserId, groupId);
        }
    }
}
