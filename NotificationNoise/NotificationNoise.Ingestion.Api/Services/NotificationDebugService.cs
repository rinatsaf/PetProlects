using Contracts.Notifications;
using NotificationNoise.BuildingBlocks;

namespace NotificationNoise.Ingestion.Api.Services;

public sealed class NotificationDebugService
{
    private readonly IEventPublisher _publisher;

    public NotificationDebugService(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<Guid> PublishReceivedAsync(CancellationToken ct)
    {
        var notificationId = Guid.NewGuid();
        var ev = new NotificationReceived(
            NotificationId: notificationId,
            UserId: "demo",
            Provider: "gmail",
            ExternalMessageId: Guid.NewGuid().ToString("N"),
            FromEmail: "no-reply@github.com",
            FromName: "GitHub",
            Subject: "Manual test",
            Snippet: "Hello from manual publish",
            ReceivedAt: DateTimeOffset.UtcNow,
            HasListUnsubscribe: false
        );

        await _publisher.PublishAsync("notification.received", ev, ct);
        return notificationId;
    }
}
