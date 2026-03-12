using Microsoft.AspNetCore.Mvc;
using NotificationNoise.Ingestion.Application;

namespace NotificationNoise.Ingestion.Api.Controllers;

[ApiController]
[Route("ingestion")]
public sealed class IngestionController : ControllerBase
{
    private readonly GmailBackfillService _backfillService;

    public IngestionController(GmailBackfillService backfillService)
    {
        _backfillService = backfillService;
    }

    [HttpPost("backfill")]
    public async Task<IActionResult> Backfill(
        [FromQuery] string userId,
        [FromQuery] int days = 30,
        [FromQuery] int maxMessages = 200,
        [FromQuery] string? label = null,
        CancellationToken ct = default)
    {
        var count = await _backfillService.BackfillAsync(userId, days, maxMessages, label, ct);
        return Ok(new BackfillResponse(userId, count));
    }

    public sealed record BackfillResponse(string UserId, int Count);
}
