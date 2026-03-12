using NotificationNoise.Ingestion.Domain;

namespace NotificationNoise.Ingestion.Application;

public interface INotificationRepository
{
    Task<bool> ExistsAsync(string userId, string provider, string externalMessageId, CancellationToken ct);
    void Add(Notification notification);
    Task SaveChangesAsync(CancellationToken ct);
}
