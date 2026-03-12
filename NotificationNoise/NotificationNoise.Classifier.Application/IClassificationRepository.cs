using NotificationNoise.Classifier.Domain;

namespace NotificationNoise.Classifier.Application;

public interface IClassificationRepository
{
    Task<bool> ExistsAsync(string userId, string provider, string externalMessageId, CancellationToken ct);
    void Add(Classification classification);
    Task<int> CountAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
