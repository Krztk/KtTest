using KtTest.Application_Services;
using KtTest.Dtos.Auth;
using KtTest.Infrastructure.Data;
using KtTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KtTest.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : CustomControllerBase
    {
        private readonly AuthOrchestrator authOrchestrator;

        public AuthController(AuthOrchestrator authOrchestrator)
        {
            this.authOrchestrator = authOrchestrator;
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await authOrchestrator.Login(loginDto);
            return ActionResult(result);
        }

        [HttpPost, Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await authOrchestrator.RegisterOrganizationOwner(registerDto);
            return ActionResult(result);
        }

        [HttpPost, Route("register/member")]
        public async Task<IActionResult> RegisterWithCode([FromQuery] string code, [FromBody] RegisterDto registerDto)
        {
            var result = await authOrchestrator.RegisterRegularUser(code, registerDto);
            return ActionResult(result);
        }
    }
}
