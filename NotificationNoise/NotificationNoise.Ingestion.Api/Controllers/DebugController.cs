using Microsoft.AspNetCore.Mvc;
using NotificationNoise.Ingestion.Api.Services;

namespace NotificationNoise.Ingestion.Api.Controllers;

[ApiController]
[Route("debug")]
public sealed class DebugController : ControllerBase
{
    private readonly NotificationDebugService _debugService;

    public DebugController(NotificationDebugService debugService)
    {
        _debugService = debugService;
    }

    [HttpPost("publish-received")]
    public async Task<IActionResult> PublishReceived(CancellationToken ct)
    {
        var notificationId = await _debugService.PublishReceivedAsync(ct);
        return Ok(new PublishReceivedResponse(true, notificationId));
    }

    public sealed record PublishReceivedResponse(bool Published, Guid NotificationId);
}
