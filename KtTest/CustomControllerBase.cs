using KtTest.Results;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest
{
    public class CustomControllerBase : ControllerBase
    {
        protected IActionResult ActionResult(OperationResult result)
        {
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return PrepareFailureResult(result.Failures);
            }
        }

        protected IActionResult ActionResult<T>(OperationResult<T> result)
        {
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            else
            {
                return PrepareFailureResult(result.Failures);
            }
        }

        private IActionResult PrepareFailureResult(IEnumerable<Failure> failures)
        {
            var firstFailure = failures.First();
            switch (firstFailure.Status)
            {
                case FailureType.BadRequest:
                    return BadRequest(firstFailure.Description);
                case FailureType.NotFound:
                    return NotFound(firstFailure.Description);
                case FailureType.Unauthorized:
                    return Unauthorized(firstFailure.Description);
                default:
                    return BadRequest();
            }
        }
    }
}
