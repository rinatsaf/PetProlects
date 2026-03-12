namespace NotificationNoise.DigestSender.Application;

public interface IInsightsClient
{
    Task<DigestSnapshot> GetSnapshotAsync(string userId, int days, int top, CancellationToken ct);
}
