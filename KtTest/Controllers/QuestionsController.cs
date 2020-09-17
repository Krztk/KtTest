﻿using KtTest.Application_Services;
using KtTest.Dtos.Wizard;
using KtTest.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KtTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
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

        [HttpGet]
        public async Task<IActionResult> GetQuestions([FromQuery]Pagination pagination)
        {
            var result = await questionOrchestrator.GetQuestions(pagination);
            return ActionResult(result);
        }
    }
}
