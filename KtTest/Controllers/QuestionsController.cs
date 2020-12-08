using KtTest.Application_Services;
using KtTest.Dtos.Wizard;
using KtTest.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KtTest.Controllers
{
    [Authorize(Policy = "EmployeeOnly")]
    [ApiController]
    [Route("[controller]")]
    public class QuestionsController : CustomControllerBase
    {
        private readonly QuestionOrchestrator questionOrchestrator;

        public QuestionsController(QuestionOrchestrator questionOrchestrator)
        {
            this.questionOrchestrator = questionOrchestrator;
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion(QuestionDto questionDto)
        {
            var result = await questionOrchestrator.CreateQuestion(questionDto);
            return ActionResult(result);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, QuestionDto questionDto)
        {
            var result = await questionOrchestrator.UpdateQuestion(id, questionDto);
            return ActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestion(int id)
        {
            var result = await questionOrchestrator.GetQuestion(id);
            return ActionResult(result);
        }

        [HttpGet]
        public IActionResult GetQuestions([FromQuery]Pagination pagination)
        {
            var result = questionOrchestrator.GetQuestions(pagination);
            return ActionResult(result);
        }

        [HttpGet("headers")]
        public async Task<IActionResult> GetQuestionHeaders([FromQuery]Pagination pagination)
        {
            var result = await questionOrchestrator.GetQuestionHeaders(pagination);
            return ActionResult(result);
        }
    }
}
