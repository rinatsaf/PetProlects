namespace NotificationNoise.Ingestion.Application;

public interface IGmailMessageClient
{
    Task<IReadOnlyList<string>> ListMessageIdsAsync(
        string accessToken,
        string query,
        int maxMessages,
        CancellationToken ct);

    Task<GmailMessageMetadata?> GetMessageAsync(string accessToken, string messageId, CancellationToken ct);
}
