using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MemoryLeak.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("TestController.Get.Entry");
                return Ok("Hello World");
            }
            finally
            {
                _logger.LogInformation("TestController.Get.Exit");
            }
        }
    }
}
