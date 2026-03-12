namespace NotificationNoise.Insights.Domain;

public sealed class SenderStats
{
    public string UserId { get; private set; } = default!;
    public string SenderKey { get; private set; } = default!;
    public int TotalCount { get; private set; }
    public int NoiseCount { get; private set; }
    public int UsefulCount { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }

    private SenderStats() { }

    public SenderStats(string userId, string senderKey, DateTimeOffset seenAt, bool isNoise)
    {
        UserId = userId;
        SenderKey = senderKey;
        ApplyNotification(isNoise, seenAt);
    }

    public void ApplyNotification(bool isNoise, DateTimeOffset seenAt)
    {
        TotalCount++;
        if (isNoise) NoiseCount++; else UsefulCount++;
        LastSeenAt = seenAt;
    }
}
