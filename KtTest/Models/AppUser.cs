using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace KtTest.Models
{
    public class AppUser : IdentityUser<int>
    {
        public int? InvitedBy { get; private set; }
        public bool IsTeacher { get; private set; }
        public ICollection<GroupMember> GroupMembers { get; private set; } = new List<GroupMember>();

        private AppUser(string email, string userName, bool isTeacher)
        {
            Email = email;
            IsTeacher = isTeacher;
            UserName = userName;
        }

        private AppUser(string email, string userName, bool isTeacher, int? invitedBy)
            : this(email, userName, isTeacher)
        {
            InvitedBy = invitedBy;
        }

        private AppUser()
        {

        }

        public static AppUser CreateOrganizationOwner(string email, string userName)
        {
            return new AppUser(email, userName, true);
        }

        public static AppUser CreateOrganizationOwner(int id, string email, string userName)
        {
            var user = CreateOrganizationOwner(email, userName);
            user.Id = id;
            return user;
        }

        public static AppUser CreateOrganizationMember(string email, string userName, bool isTeacher, int invitedBy)
        {
            return new AppUser(email, userName, isTeacher, invitedBy);
        }

        public static AppUser CreateOrganizationMember(int id, string email, string userName, bool isTeacher, int invitedBy)
        {
            var user = CreateOrganizationMember(email, userName, isTeacher, invitedBy);
            user.Id = id;
            return user;
        }
    }
}
