using Microsoft.AspNetCore.Mvc;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHealth()
        {
            _logger.LogInformation("Health check called");

            var response = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "Order Microservice",
                version = "1.0.0"
            };

            return Ok(response);
        }

        /// <summary>
        /// Readiness probe cho Kubernetes
        /// </summary>
        [HttpGet("ready")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public IActionResult GetReadiness()
        {
            return Ok(new { ready = true });
        }

        /// <summary>
        /// Liveness probe cho Kubernetes
        /// </summary>
        [HttpGet("live")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetLiveness()
        {
            return Ok(new { alive = true });
        }
    }
}
