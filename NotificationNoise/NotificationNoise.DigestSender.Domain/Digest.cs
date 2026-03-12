namespace NotificationNoise.DigestSender.Domain;

public sealed class Digest
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string UserId { get; private set; } = default!;
    public string Period { get; private set; } = default!;
    public string PayloadJson { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }

    private Digest() { }

    public Digest(string userId, string period, string payloadJson, DateTimeOffset createdAt)
    {
        UserId = userId;
        Period = period;
        PayloadJson = payloadJson;
        CreatedAt = createdAt;
    }

    public void MarkSent(DateTimeOffset sentAt)
    {
        SentAt = sentAt;
    }
}
