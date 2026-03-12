namespace NotificationNoise.Insights.Domain;

public enum RecommendationType
{
    Unsubscribe,
    Mute,
    Filter,
    Digest
}

public enum RecommendationStatus
{
    New,
    Applied,
    Dismissed
}

public sealed class Recommendation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = default!;
    public RecommendationType Type { get; private set; }
    public string Target { get; private set; } = default!;
    public string Why { get; private set; } = default!;
    public int ImpactScore { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public RecommendationStatus Status { get; private set; } = RecommendationStatus.New;

    private Recommendation() { }

    public Recommendation(
        string userId,
        RecommendationType type,
        string target,
        string why,
        int impactScore,
        DateTimeOffset createdAt)
    {
        UserId = userId;
        Type = type;
        Target = target;
        Why = why;
        ImpactScore = impactScore;
        CreatedAt = createdAt;
    }

    public void Dismiss() => Status = RecommendationStatus.Dismissed;
    public void Apply() => Status = RecommendationStatus.Applied;
}
