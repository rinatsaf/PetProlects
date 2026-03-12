using Microsoft.AspNetCore.Mvc;

namespace NotificationNoise.Insights.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("OK");
    }
}
