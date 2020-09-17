using KtTest.Dtos.Organizations;
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

        public OrganizationOrchestrator(OrganizationService organizationService)
        {
            this.organizationService = organizationService;
        }

        public async Task<OperationResult> InviteUser(InviteUserDto inviteUserDto)
        {
            return await organizationService.CreateRegistrationInvitation(inviteUserDto.Email, inviteUserDto.IsTeacher);
        }
    }
}
