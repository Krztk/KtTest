using KtTest.Results;
using KtTest.Results.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                return PrepareFailureResult(result.Error);
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
                return PrepareFailureResult(result.Error);
            }
        }

        private IActionResult PrepareFailureResult(ErrorBase error)
        {
            return error switch
            {
                BadRequestError _ => BadRequest(error),
                AuthorizationError _ => Unauthorized(error),
                DataNotFoundError _ => NotFound(error),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
