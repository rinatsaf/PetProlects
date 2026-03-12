using Microsoft.EntityFrameworkCore;
using NotificationNoise.Classifier.Application;
using NotificationNoise.Classifier.Domain;

namespace NotificationNoise.Classifier.Infrastructure.Persistence;

public sealed class ClassificationRepository : IClassificationRepository
{
    private readonly ClassifierDbContext _db;

    public ClassificationRepository(ClassifierDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(string userId, string provider, string externalMessageId, CancellationToken ct)
    {
        return _db.Classifications.AnyAsync(
            x => x.UserId == userId && x.Provider == provider && x.ExternalMessageId == externalMessageId,
            ct);
    }

    public void Add(Classification classification)
    {
        _db.Classifications.Add(classification);
    }

    public Task<int> CountAsync(CancellationToken ct)
    {
        return _db.Classifications.CountAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}
