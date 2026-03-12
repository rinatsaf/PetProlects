using Microsoft.Extensions.Logging;
using NotificationNoise.DigestSender.Application;

namespace NotificationNoise.DigestSender.Infrastructure;

public sealed class LogDigestDelivery : IDigestDelivery
{
    private readonly ILogger<LogDigestDelivery> _logger;

    public LogDigestDelivery(ILogger<LogDigestDelivery> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string userId, string period, string payloadJson, CancellationToken ct)
    {
        _logger.LogInformation("Digest for {UserId} ({Period}): {Payload}", userId, period, payloadJson);
        return Task.CompletedTask;
    }
}
