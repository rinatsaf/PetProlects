using Microsoft.AspNetCore.Mvc;
using NotificationNoise.Insights.Application;

namespace NotificationNoise.Insights.Api.Controllers;

[ApiController]
[Route("stats")]
public sealed class StatsController : ControllerBase
{
    private readonly InsightsQueryService _queryService;

    public StatsController(InsightsQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet("top-senders")]
    public async Task<IActionResult> GetTopSenders([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var items = await _queryService.GetTopSendersAsync(limit, ct);
        return Ok(items);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var items = await _queryService.GetTrendsAsync(days, ct);
        return Ok(items);
    }
}
