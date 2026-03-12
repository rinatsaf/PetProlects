namespace NotificationNoise.Ingestion.Domain;

public sealed class Notification
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string UserId { get; private set; } = default!;
    public string Provider { get; private set; } = "gmail";
    public string ExternalMessageId { get; private set; } = default!;

    public string FromEmail { get; private set; } = default!;
    public string? FromName { get; private set; }
    public string? Subject { get; private set; }
    public string? Snippet { get; private set; }

    public DateTimeOffset ReceivedAt { get; private set; }
    public bool HasListUnsubscribe { get; private set; }

    // EF
    private Notification() { }

    public Notification(
        string userId,
        string provider,
        string externalMessageId,
        string fromEmail,
        string? fromName,
        string? subject,
        string? snippet,
        DateTimeOffset receivedAt,
        bool hasListUnsubscribe)
    {
        UserId = userId;
        Provider = provider;
        ExternalMessageId = externalMessageId;
        FromEmail = fromEmail;
        FromName = fromName;
        Subject = subject;
        Snippet = snippet;
        ReceivedAt = receivedAt;
        HasListUnsubscribe = hasListUnsubscribe;
    }
}