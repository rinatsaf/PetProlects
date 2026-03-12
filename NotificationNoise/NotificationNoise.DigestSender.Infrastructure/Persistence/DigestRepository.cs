using NotificationNoise.DigestSender.Application;
using NotificationNoise.DigestSender.Domain;

namespace NotificationNoise.DigestSender.Infrastructure.Persistence;

public sealed class DigestRepository : IDigestRepository
{
    private readonly DigestDbContext _db;

    public DigestRepository(DigestDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(Digest digest, CancellationToken ct)
    {
        return _db.Digests.AddAsync(digest, ct).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}
