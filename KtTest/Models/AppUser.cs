using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace KtTest.Models
{
    public class AppUser : IdentityUser<int>
    {
        public int? InvitedBy { get; set; }
        public bool IsTeacher { get; set; }
        public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    }
}
