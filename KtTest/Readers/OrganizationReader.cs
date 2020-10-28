using KtTest.Dtos.Organizations;
using KtTest.Infrastructure.Data;
using KtTest.Infrastructure.Mappers;
using KtTest.Results;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Readers
{
    public class OrganizationReader
    {
        private readonly ReadOnlyAppDbContext dbContext;
        private readonly OrganizationServiceMapper organizationMapper;

        public OrganizationReader(ReadOnlyAppDbContext dbContext, OrganizationServiceMapper organizationMapper)
        {
            this.dbContext = dbContext;
            this.organizationMapper = organizationMapper;
        }

        public List<UserDto> GetOrganizationMembers(int organizationCreatorId)
        {
            return dbContext.Users
                .Where(u => u.InvitedBy == organizationCreatorId)
                .OrderBy(x => x.UserName)
                .Select(organizationMapper.MapToUserDto)
                .ToList();
        }

        public List<InvitationDto> GetInvitations(int organizationCreatorId)
        {
            return dbContext.Invitations
                .Where(x => x.InvitedBy == organizationCreatorId)
                .OrderByDescending(x => x.Email)
                .Select(organizationMapper.MapToInvitationDto)
                .ToList();
        }
    }
}
