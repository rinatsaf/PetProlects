using NotificationNoise.Insights.Domain;

namespace NotificationNoise.Insights.Application;

public interface IInsightsRepository
{
    Task<List<TopSenderItem>> GetTopSendersAsync(int limit, CancellationToken ct);
    Task<List<TrendItem>> GetTrendsAsync(int days, CancellationToken ct);
    Task<List<RecommendationListItem>> GetRecommendationsAsync(RecommendationStatus status, CancellationToken ct);
    Task<bool> DismissRecommendationAsync(Guid id, CancellationToken ct);
    Task<SenderStats?> GetSenderStatsAsync(string userId, string senderKey, CancellationToken ct);
    void AddSenderStats(SenderStats senderStats);
    Task<DailyStats?> GetDailyStatsAsync(string userId, DateTime date, CancellationToken ct);
    void AddDailyStats(DailyStats dailyStats);
    Task<bool> HasRecommendationAsync(
        string userId,
        RecommendationType type,
        string target,
        RecommendationStatus status,
        CancellationToken ct);
    void AddRecommendation(Recommendation recommendation);
    Task SaveChangesAsync(CancellationToken ct);
}
