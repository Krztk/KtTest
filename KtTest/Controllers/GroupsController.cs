using KtTest.Application_Services;
using KtTest.Dtos.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Controllers
{
    [Authorize(Policy = "OwnerOnly")]
    [ApiController]
    [Route("[controller]")]
    public class GroupsController : CustomControllerBase
    {
        private readonly GroupOrchestrator groupOrchestrator;

        public GroupsController(GroupOrchestrator groupOrchestrator)
        {
            this.groupOrchestrator = groupOrchestrator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(CreateGroupDto createGroupDto)
        {
            var result = await groupOrchestrator.CreateGroup(createGroupDto);
            return ActionResult(result);
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, AddMemberDto addMemberDto)
        {
            var result = await groupOrchestrator.AddMemberToGroup(id, addMemberDto);
            return ActionResult(result);
        }
    }
}
