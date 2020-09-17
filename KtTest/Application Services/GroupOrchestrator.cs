using KtTest.Dtos.Groups;
using KtTest.Models;
using KtTest.Results;
using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class GroupOrchestrator
    {
        private readonly GroupService groupService;
        private readonly OrganizationService organizationService;
        private readonly IUserContext userContext;

        public GroupOrchestrator(GroupService groupService, OrganizationService organizationService, IUserContext userContext)
        {
            this.groupService = groupService;
            this.organizationService = organizationService;
            this.userContext = userContext;
        }

        public async Task<OperationResult<int>> CreateGroup(CreateGroupDto createGroupDto)
        {
            return await groupService.CreateGroup(createGroupDto.Name);
        }

        public async Task<OperationResult> AddMemberToGroup(int groupId, AddMemberDto addMemberDto)
        {
            int owner = userContext.UserId;
            var isMember = await organizationService.IsUserMemberOfOrganization(owner, addMemberDto.UserId);
            
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
