using KtTest.Application_Services;
using KtTest.Dtos.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KtTest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GroupsController : CustomControllerBase
    {
        private readonly GroupOrchestrator groupOrchestrator;

        public GroupsController(GroupOrchestrator groupOrchestrator)
        {
            this.groupOrchestrator = groupOrchestrator;
        }

        [HttpGet]
        public IActionResult GetGroups()
        {
            var result = groupOrchestrator.GetGroupHeaders();
            return Ok(result);
        }

        [Authorize(Policy = "OwnerOnly")]
        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetGroupMembers(int id)
        {
            var result = await groupOrchestrator.GetGroupMembers(id);
            return ActionResult(result);
        }

        [Authorize(Policy = "OwnerOnly")]
        [HttpGet("{id}/available")]
        public async Task<IActionResult> GetAvailableUsers(int id)
        {
            var result = await groupOrchestrator.GetAvailableUsers(id);
            return ActionResult(result);
        }

        [Authorize(Policy = "OwnerOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateGroup(CreateGroupDto createGroupDto)
        {
            var result = await groupOrchestrator.CreateGroup(createGroupDto);
            return ActionResult(result);
        }

        [Authorize(Policy = "OwnerOnly")]
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, AddMemberDto addMemberDto)
        {
            var result = await groupOrchestrator.AddMemberToGroup(id, addMemberDto);
            return ActionResult(result);
        }
    }
}
