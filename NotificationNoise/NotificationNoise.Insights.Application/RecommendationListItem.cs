using NotificationNoise.Insights.Domain;

namespace NotificationNoise.Insights.Application;

public sealed record RecommendationListItem(
    Guid Id,
    RecommendationType Type,
    string Target,
    string Why,
    int ImpactScore,
    DateTimeOffset CreatedAt,
    RecommendationStatus Status);
