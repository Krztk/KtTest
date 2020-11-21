using KtTest.Application_Services;
using KtTest.Dtos.Test;
using KtTest.Dtos.Wizard;
using KtTest.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KtTest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TestsController : CustomControllerBase
    {
        private readonly TestOrchestrator testOrchestrator;

        public TestsController(TestOrchestrator testOrchestrator)
        {
            this.testOrchestrator = testOrchestrator;
        }

        [HttpGet]
        public IActionResult GetAvailableTests([FromQuery] Pagination pagination)
        {
            var result = testOrchestrator.GetAvailableAndUpcomingTests(pagination);
            return ActionResult(result);
        }

        [Authorize(Policy = "EmployeeOnly")]
        [HttpGet("wizard")]
        public IActionResult GetTests([FromQuery] Pagination pagination)
        {
            var result = testOrchestrator.GetTestTemplates(pagination);
            return ActionResult(result);
        }

        [Authorize(Policy = "EmployeeOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateTestTemplate(CreateTestDto createTestDto)
        {
            var result = await testOrchestrator.CreateTestTemplate(createTestDto);
            return ActionResult(result);
        }

        [Authorize(Policy = "EmployeeOnly")]
        [HttpPost("{id}/publish")]
        public async Task<IActionResult> ScheduleTest(int id, PublishTestDto publishTestDto)
        {
            var result = await testOrchestrator.ScheduleTest(id, publishTestDto);
            return ActionResult(result);
        }

        [Authorize(Policy = "EmployeeOnly")]
        [HttpGet("wizard/{id}")]
        public IActionResult GetTestWizard(int id)
        {
            var result = testOrchestrator.GetTestWizard(id);
            return ActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTest(int id)
        {
            var result = await testOrchestrator.GetTest(id);
            return ActionResult(result);
        }

        [HttpPost("{id}/answers")]
        public async Task<IActionResult> SendAnswer(int id, SendTestAnswersDto sendAnswersDto)
        {
            var result = await testOrchestrator.AddUserAnswers(id, sendAnswersDto);
            return ActionResult(result);
        }

        [HttpGet("{id}/answers")]
        public async Task<IActionResult> GetAnswers(int id)
        {
            var result = await testOrchestrator.GetUserAnswers(id);
            return ActionResult(result);
        }

        [HttpGet("{id}/result")]
        public async Task<IActionResult> GetTestResults(int id)
        {
            var result = await testOrchestrator.GetTestResult(id);
            return ActionResult(result);
        }

        [Authorize(Policy = "EmployeeOnly")]
        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetTestResultsTeacher(int id)
        {
            var result = await testOrchestrator.GetTestResultTeacher(id);
            return ActionResult(result);
        }
    }
}

