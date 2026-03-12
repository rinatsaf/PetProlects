using Microsoft.AspNetCore.Mvc;
using NotificationNoise.Classifier.Application;

namespace NotificationNoise.Classifier.Api.Controllers;

[ApiController]
[Route("debug/classifications")]
public sealed class DebugController : ControllerBase
{
    private readonly NotificationClassificationService _classificationService;

    public DebugController(NotificationClassificationService classificationService)
    {
        _classificationService = classificationService;
    }

    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        var count = await _classificationService.GetCountAsync(ct);
        return Ok(new CountResponse(count));
    }

    public sealed record CountResponse(int Count);
}
