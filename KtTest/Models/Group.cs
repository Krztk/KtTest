using System.Collections.Generic;

namespace KtTest.Models
{
    public class Group
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        private readonly List<GroupMember> groupMembers = new List<GroupMember>();
        public IReadOnlyCollection<GroupMember> GroupMembers => groupMembers.AsReadOnly();
        public int OwnerId { get; private set; }
        public Group(string name, int ownerId)
        {
            Name = name;
            OwnerId = ownerId;
            groupMembers.Add(new GroupMember(ownerId, Id));
        }

        public Group(int id, string name, int ownerId) : this(name, ownerId)
        {
            Id = id;
        }

        public void AddMember(int userId)
        {
            groupMembers.Add(new GroupMember(userId, Id));
        }

        private Group()
        {

        }
    }

    public class GroupMember
    {
        public int UserId { get; private set; }
        public AppUser User { get; private set; }
        public int GroupId { get; private set; }
        public Group Group { get; private set; }

        public GroupMember(int userId, int groupId)
        {
            UserId = userId;
            GroupId = groupId;
        }
    }
}
