using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
using KtTest.Results.Errors;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public class GroupService
    {
        private readonly AppDbContext dbContext;
        private readonly IUserContext userContext;

        public GroupService(AppDbContext dbContext, IUserContext userContext)
        {
            this.dbContext = dbContext;
            this.userContext = userContext;
        }

        public async Task<OperationResult<int>> CreateGroup(string name)
        {
            var group = new Group(name, userContext.UserId);
            group.GroupMembers.Add(new GroupMember { UserId = userContext.UserId });
            dbContext.Groups.Add(group);
            await dbContext.SaveChangesAsync();
            return group.Id;
        }

        public async Task<OperationResult<Unit>> AddUserToGroup(int userId, int groupId)
        {
            var hasUserJoinedGroup = await dbContext.GroupMembers.Where(x => x.UserId == userId && x.GroupId == groupId).FirstOrDefaultAsync() != null;

            if (hasUserJoinedGroup)
            {
                return new BadRequestError();
            }

            var groupMember = new GroupMember { GroupId = groupId, UserId = userId };
            dbContext.GroupMembers.Add(groupMember);
            await dbContext.SaveChangesAsync();
            return OperationResult.Ok;
        }

        public async Task<bool> IsUserMemberOfGroup(int userId, int groupId)
        {
            return await dbContext.GroupMembers
                .Where(x => x.UserId == userId && x.GroupId == groupId)
                .FirstOrDefaultAsync() != null;
        }

        public async Task<List<UserInfo>> GetStudentsFromGroup(int groupId)
        {
            var groupMembers = await dbContext.GroupMembers
                .Where(x => x.GroupId == groupId)
                .Include(x => x.User)
                .Select(x => x.User)
                .Select(x => new UserInfo(x.Id, x.IsTeacher))
                .ToListAsync();

            groupMembers.RemoveAll(x => x.IsTeacher);
            return groupMembers;
        }

        public async Task<OperationResult<int>> GetIdOfGroupOwner(int groupId)
        {
            var group = await dbContext.Groups.Where(x => x.Id == groupId).FirstOrDefaultAsync();
            if (group == null)
            {
                return new BadRequestError();
            }

            return group.OwnerId;
        }
    }
}
