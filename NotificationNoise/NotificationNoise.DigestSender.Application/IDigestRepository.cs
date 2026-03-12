using NotificationNoise.DigestSender.Domain;

namespace NotificationNoise.DigestSender.Application;

public interface IDigestRepository
{
    Task AddAsync(Digest digest, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
