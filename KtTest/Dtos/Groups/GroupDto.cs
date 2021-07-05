using KtTest.Dtos.Organizations;
using System.Collections.Generic;

namespace KtTest.Dtos.Groups
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UserDto> GroupMembers { get; set; }
    }
}
