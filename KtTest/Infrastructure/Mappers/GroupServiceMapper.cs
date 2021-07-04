using KtTest.Dtos.Groups;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Infrastructure.Mappers
{
    public class GroupServiceMapper
    {
        public GroupHeaderDto MapToGroupHeader(Group group)
        {
            return new GroupHeaderDto
            {
                Id = group.Id,
                Name = group.Name
            };
        }
    }
}
