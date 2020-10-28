using KtTest.Dtos.Organizations;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Infrastructure.Mappers
{
    public class OrganizationServiceMapper
    {
        public UserDto MapToUserDto(AppUser appUser)
        {
            return new UserDto
            {
                Email = appUser.Email,
                Id = appUser.Id,
                IsTeacher = appUser.IsTeacher,
                Username = appUser.UserName
            };
        }

        public InvitationDto MapToInvitationDto(Invitation invitation)
        {
            return new InvitationDto
            {
                Email = invitation.Email,
                Date = invitation.Date,
                IsTeacher = invitation.IsTeacher
            };
        }
    }
}
