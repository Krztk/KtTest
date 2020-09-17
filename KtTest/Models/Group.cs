using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Models
{
    public class Group
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public ICollection<GroupMember> GroupMembers { get; private set; } = new List<GroupMember>();
        public int OwnerId { get; set; }
        public Group(string name, int ownerId)
        {
            Name = name;
            OwnerId = ownerId;
        }

        private Group()
        {

        }
    }

    public class GroupMember
    {
        public int UserId { get; set; }
        public AppUser User { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}
