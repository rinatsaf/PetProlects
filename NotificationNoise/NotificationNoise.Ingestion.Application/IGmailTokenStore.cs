namespace NotificationNoise.Ingestion.Application;

public interface IGmailTokenStore
{
    Task<GmailTokenData?> GetAsync(string userId, CancellationToken ct);
    Task UpsertAsync(GmailTokenData data, CancellationToken ct);
}
