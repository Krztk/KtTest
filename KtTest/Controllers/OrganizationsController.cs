﻿using KtTest.Application_Services;
using KtTest.Dtos.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class OrganizationsController : CustomControllerBase
    {
        private readonly OrganizationOrchestrator organizationOrchestrator;

        public OrganizationsController(OrganizationOrchestrator organizationOrchestrator)
        {
            this.organizationOrchestrator = organizationOrchestrator;
        }

        [HttpPost("invite")]
        public async Task<IActionResult> InviteUser(InviteUserDto dto)
        {
            var result = await organizationOrchestrator.InviteUser(dto);
            return ActionResult(result);
        }
    }
}
