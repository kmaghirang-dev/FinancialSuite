using Microsoft.AspNetCore.Mvc;

namespace FinancialSuite.API.Controllers
{
    [ApiController]
    [Route("api/error-test")]
    public class ErrorTestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Throw()
        {
            throw new InvalidOperationException("Test exception from ErrorTestController.");
        }
    }
}
