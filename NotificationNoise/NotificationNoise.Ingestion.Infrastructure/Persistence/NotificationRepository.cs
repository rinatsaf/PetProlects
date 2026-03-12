using Microsoft.EntityFrameworkCore;
using NotificationNoise.Ingestion.Application;
using NotificationNoise.Ingestion.Domain;

namespace NotificationNoise.Ingestion.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly IngestionDbContext _db;

    public NotificationRepository(IngestionDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(string userId, string provider, string externalMessageId, CancellationToken ct)
    {
        return _db.Notifications.AnyAsync(
            x => x.UserId == userId && x.Provider == provider && x.ExternalMessageId == externalMessageId,
            ct);
    }

    public void Add(Notification notification)
    {
        _db.Notifications.Add(notification);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}
