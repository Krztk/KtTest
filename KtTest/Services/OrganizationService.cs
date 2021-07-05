using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
using KtTest.Results.Errors;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public class OrganizationService
    {
        private readonly AppDbContext dbContext;
        private readonly IUserContext userContext;
        private readonly IRegistrationCodeGenerator codeGenerator;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IEmailSender emailSender;

        public OrganizationService(AppDbContext dbContext, IUserContext userContext, IRegistrationCodeGenerator codeGenerator, IDateTimeProvider dateTimeProvider, IEmailSender emailSender)
        {
            this.dbContext = dbContext;
            this.userContext = userContext;
            this.codeGenerator = codeGenerator;
            this.dateTimeProvider = dateTimeProvider;
            this.emailSender = emailSender;
        }
        public async Task<OperationResult<int>> CreateRegistrationInvitation(string email, bool isTeacher)
        {
            var code = codeGenerator.GenerateCode();
            var invitation = new Invitation(email,
                                            isTeacher,
                                            code,
                                            userContext.UserId,
                                            dateTimeProvider.UtcNow);

            dbContext.Invitations.Add(invitation);
            await dbContext.SaveChangesAsync();

            await emailSender.SendEmail(email, code);
            return invitation.Id;
        }

        public async Task<bool> IsUserMemberOfOrganization(int ownerId, int userId)
        {
            if (ownerId == userId)
                return true;

            return await dbContext.Users.Where(x => x.Id == userId && x.InvitedBy == ownerId).CountAsync() > 0;
        }

        public async Task<OperationResult<UserInfo>> GetMember(int organizationOwner, int userId)
        {
            var user = await dbContext.Users
                .Where(x => x.Id == userId && x.InvitedBy == organizationOwner)
                .Select(x=>new UserInfo(x.Id, x.IsTeacher))
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new BadRequestError();
            }

            return user;
        }
    }
}
