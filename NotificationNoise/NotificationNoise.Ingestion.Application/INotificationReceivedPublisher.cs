using NotificationNoise.Ingestion.Domain;

namespace NotificationNoise.Ingestion.Application;

public interface INotificationReceivedPublisher
{
    Task PublishAsync(Notification notification, CancellationToken ct);
}
