using KtTest.Dtos.Organizations;
using KtTest.Readers;
using KtTest.Results;
using KtTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class OrganizationOrchestrator
    {
        private readonly OrganizationService organizationService;
        private readonly OrganizationReader organizationReader;
        private readonly IUserContext userContext;

        public OrganizationOrchestrator(OrganizationService organizationService, OrganizationReader organizationReader, IUserContext userContext)
        {
            this.organizationService = organizationService;
            this.organizationReader = organizationReader;
            this.userContext = userContext;
        }

        public async Task<OperationResult<int>> InviteUser(InviteUserDto inviteUserDto)
        {
            return await organizationService.CreateRegistrationInvitation(inviteUserDto.Email, inviteUserDto.IsTeacher);
        }

        public List<UserDto> GetOrganizationMembers()
        {
            return organizationReader.GetOrganizationMembers(userContext.UserId);
        }

        public List<InvitationDto> GetInvitations()
        {
            return organizationReader.GetInvitations(userContext.UserId);
        }
    }
}
