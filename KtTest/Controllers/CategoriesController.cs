using KtTest.Application_Services;
using KtTest.Dtos.Wizard;
using KtTest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Controllers
{
    [Authorize(Policy = "EmployeeOnly")]
    [ApiController]
    [Route("[controller]")]
    public class CategoriesController : CustomControllerBase
    {
        private readonly CategoryOrchestrator categoryOrchestrator;

        public CategoriesController(CategoryOrchestrator categoryOrchestrator)
        {
            this.categoryOrchestrator = categoryOrchestrator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var result = await categoryOrchestrator.GetCategories();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            var result = await categoryOrchestrator.CreateCategory(createCategoryDto);
            return ActionResult(result);
        }
    }
}
