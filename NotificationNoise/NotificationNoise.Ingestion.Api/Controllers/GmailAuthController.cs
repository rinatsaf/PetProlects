using Microsoft.AspNetCore.Mvc;
using NotificationNoise.Ingestion.Application;

namespace NotificationNoise.Ingestion.Api.Controllers;

[ApiController]
[Route("auth/gmail")]
public sealed class GmailAuthController : ControllerBase
{
    private readonly GmailOAuthService _oauthService;

    public GmailAuthController(GmailOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    [HttpPost("connect")]
    public IActionResult Connect([FromQuery] string userId)
    {
        var url = _oauthService.BuildAuthUrl(userId);
        return Ok(new ConnectResponse(url));
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string? state,
        [FromQuery] string? userId,
        CancellationToken ct)
    {
        var resolvedUserId = state ?? userId;
        if (string.IsNullOrWhiteSpace(resolvedUserId))
        {
            return BadRequest(new { error = "userId missing" });
        }

        await _oauthService.ExchangeCodeAsync(resolvedUserId, code, ct);
        return Ok(new CallbackResponse(true, resolvedUserId));
    }

    public sealed record ConnectResponse(string Url);
    public sealed record CallbackResponse(bool Connected, string UserId);
}
