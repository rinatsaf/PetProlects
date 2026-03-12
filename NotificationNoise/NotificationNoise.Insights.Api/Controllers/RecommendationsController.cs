using Microsoft.AspNetCore.Mvc;
using NotificationNoise.Insights.Application;

namespace NotificationNoise.Insights.Api.Controllers;

[ApiController]
[Route("recommendations")]
public sealed class RecommendationsController : ControllerBase
{
    private readonly InsightsQueryService _queryService;

    public RecommendationsController(InsightsQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string status = "new", CancellationToken ct = default)
    {
        var items = await _queryService.GetRecommendationsAsync(status, ct);
        if (items is null)
        {
            return BadRequest(new { error = "unknown status" });
        }

        return Ok(items);
    }

    [HttpPost("{id:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken ct)
    {
        var dismissed = await _queryService.DismissRecommendationAsync(id, ct);
        if (!dismissed)
        {
            return NotFound();
        }

        return Ok();
    }
}
