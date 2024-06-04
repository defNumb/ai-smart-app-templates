using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DBTransferProject.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post()
        {
            _logger.LogInformation("Test endpoint received a POST request.");
            return Ok(new { Message = "Test endpoint received a POST request." });
        }
    }
}
