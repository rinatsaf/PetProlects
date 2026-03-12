using Microsoft.AspNetCore.Mvc;
using NotificationNoise.DigestSender.Application;

namespace NotificationNoise.DigestSender.Api.Controllers;

[ApiController]
[Route("digest")]
public sealed class DigestController : ControllerBase
{
    private readonly DigestService _digestService;

    public DigestController(DigestService digestService)
    {
        _digestService = digestService;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(
        [FromQuery] string userId = "demo",
        [FromQuery] string period = "weekly",
        [FromQuery] int days = 7,
        [FromQuery] int top = 10,
        CancellationToken ct = default)
    {
        var digest = await _digestService.RunAsync(userId, period, days, top, ct);
        return Ok(new DigestResponse(
            digest.Id,
            digest.UserId,
            digest.Period,
            digest.CreatedAt,
            digest.SentAt));
    }

    public sealed record DigestResponse(
        Guid Id,
        string UserId,
        string Period,
        DateTimeOffset CreatedAt,
        DateTimeOffset? SentAt);
}
