using NotificationNoise.Insights.Domain;

namespace NotificationNoise.Insights.Application;

public sealed class InsightsQueryService
{
    private readonly IInsightsRepository _repository;

    public InsightsQueryService(IInsightsRepository repository)
    {
        _repository = repository;
    }

    public Task<List<TopSenderItem>> GetTopSendersAsync(int limit, CancellationToken ct)
    {
        return _repository.GetTopSendersAsync(limit, ct);
    }

    public Task<List<TrendItem>> GetTrendsAsync(int days, CancellationToken ct)
    {
        return _repository.GetTrendsAsync(days, ct);
    }

    public async Task<List<RecommendationListItem>?> GetRecommendationsAsync(string status, CancellationToken ct)
    {
        if (!Enum.TryParse<RecommendationStatus>(status, true, out var parsedStatus))
        {
            return null;
        }

        return await _repository.GetRecommendationsAsync(parsedStatus, ct);
    }

    public Task<bool> DismissRecommendationAsync(Guid id, CancellationToken ct)
    {
        return _repository.DismissRecommendationAsync(id, ct);
    }
}
