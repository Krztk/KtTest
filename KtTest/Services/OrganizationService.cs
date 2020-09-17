using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public class OrganizationService
    {
        private readonly AppDbContext dbContext;
        private readonly IUserContext userContext;
        private readonly IRegistrationCodeGenerator codeGenerator;
        private readonly IEmailSender emailSender;

        public OrganizationService(AppDbContext dbContext, IUserContext userContext, IRegistrationCodeGenerator codeGenerator, IEmailSender emailSender)
        {
            this.dbContext = dbContext;
            this.userContext = userContext;
            this.codeGenerator = codeGenerator;
            this.emailSender = emailSender;
        }
        public async Task<OperationResult> CreateRegistrationInvitation(string email, bool isTeacher)
        {
            var code = codeGenerator.GenerateCode();
            var invitation = new Invitation
            {
                Code = code,
                Email = email,
                IsTeacher = isTeacher,
                InvitedBy = userContext.UserId
            };

            dbContext.Invitations.Add(invitation);
            await dbContext.SaveChangesAsync();

            await emailSender.SendEmail(email, code);
            return new OperationResult();
        }

        public async Task<bool> IsUserMemberOfOrganization(int ownerId, int userId)
        {
            return await dbContext.Users.Where(x => x.Id == userId && x.InvitedBy == ownerId).CountAsync() > 0;
        }

        public async Task<OperationResult<UserInfo>> GetMember(int organizationOwner, int userId)
        {
            var result = new OperationResult<UserInfo>();
            var user = await dbContext.Users
                .Where(x => x.Id == userId && x.InvitedBy == organizationOwner)
                .Select(x=>new UserInfo(x.Id, x.IsTeacher))
                .FirstOrDefaultAsync();

            if (user == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }
            result.Data = user;
            return result;
        }
    }
}
