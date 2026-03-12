namespace NotificationNoise.DigestSender.Application;

public sealed record DigestSnapshot(
    string UserId,
    int Days,
    int Top,
    IReadOnlyList<TopSenderItem> TopSenders,
    IReadOnlyList<TrendItem> Trends,
    IReadOnlyList<RecommendationItem> Recommendations
);

public sealed record TopSenderItem(
    string SenderKey,
    int TotalCount,
    int NoiseCount,
    int UsefulCount
);

public sealed record TrendItem(
    DateTime Date,
    int Total,
    int Noise,
    int Useful
);

public sealed record RecommendationItem(
    Guid Id,
    int Type,
    string Target,
    string Why,
    int ImpactScore
);
