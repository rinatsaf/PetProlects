namespace NotificationNoise.Insights.Application;

public sealed record TopSenderItem(
    string SenderKey,
    int TotalCount,
    int NoiseCount,
    int UsefulCount,
    DateTimeOffset LastSeenAt);
