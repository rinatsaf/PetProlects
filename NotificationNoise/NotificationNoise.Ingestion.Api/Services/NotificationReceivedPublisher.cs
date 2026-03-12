using Contracts.Notifications;
using NotificationNoise.BuildingBlocks;
using NotificationNoise.Ingestion.Application;
using NotificationNoise.Ingestion.Domain;

namespace NotificationNoise.Ingestion.Api.Services;

public sealed class NotificationReceivedPublisher : INotificationReceivedPublisher
{
    private readonly IEventPublisher _publisher;

    public NotificationReceivedPublisher(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task PublishAsync(Notification notification, CancellationToken ct)
    {
        var ev = new NotificationReceived(
            NotificationId: notification.Id,
            UserId: notification.UserId,
            Provider: notification.Provider,
            ExternalMessageId: notification.ExternalMessageId,
            FromEmail: notification.FromEmail,
            FromName: notification.FromName,
            Subject: notification.Subject,
            Snippet: notification.Snippet,
            ReceivedAt: notification.ReceivedAt,
            HasListUnsubscribe: notification.HasListUnsubscribe);

        return _publisher.PublishAsync("notification.received", ev, ct);
    }
}
