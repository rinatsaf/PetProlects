using Microsoft.EntityFrameworkCore;
using NotificationNoise.Insights.Application;
using NotificationNoise.Insights.Domain;

namespace NotificationNoise.Insights.Infrastructure.Persistence;

public sealed class InsightsRepository : IInsightsRepository
{
    private readonly InsightsDbContext _db;

    public InsightsRepository(InsightsDbContext db)
    {
        _db = db;
    }

    public Task<List<TopSenderItem>> GetTopSendersAsync(int limit, CancellationToken ct)
    {
        return _db.SenderStats
            .AsNoTracking()
            .OrderByDescending(x => x.NoiseCount)
            .Take(limit)
            .Select(x => new TopSenderItem(
                x.SenderKey,
                x.TotalCount,
                x.NoiseCount,
                x.UsefulCount,
                x.LastSeenAt))
            .ToListAsync(ct);
    }

    public Task<List<TrendItem>> GetTrendsAsync(int days, CancellationToken ct)
    {
        var from = DateTime.UtcNow.Date.AddDays(-days + 1);

        return _db.DailyStats
            .AsNoTracking()
            .Where(x => x.Date >= from)
            .OrderBy(x => x.Date)
            .Select(x => new TrendItem(
                x.Date,
                x.Total,
                x.Noise,
                x.Useful))
            .ToListAsync(ct);
    }

    public Task<List<RecommendationListItem>> GetRecommendationsAsync(RecommendationStatus status, CancellationToken ct)
    {
        return _db.Recommendations
            .AsNoTracking()
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new RecommendationListItem(
                x.Id,
                x.Type,
                x.Target,
                x.Why,
                x.ImpactScore,
                x.CreatedAt,
                x.Status))
            .ToListAsync(ct);
    }

    public async Task<bool> DismissRecommendationAsync(Guid id, CancellationToken ct)
    {
        var recommendation = await _db.Recommendations.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (recommendation is null)
        {
            return false;
        }

        recommendation.Dismiss();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<SenderStats?> GetSenderStatsAsync(string userId, string senderKey, CancellationToken ct)
    {
        return _db.SenderStats.FirstOrDefaultAsync(x => x.UserId == userId && x.SenderKey == senderKey, ct);
    }

    public void AddSenderStats(SenderStats senderStats)
    {
        _db.SenderStats.Add(senderStats);
    }

    public Task<DailyStats?> GetDailyStatsAsync(string userId, DateTime date, CancellationToken ct)
    {
        return _db.DailyStats.FirstOrDefaultAsync(x => x.UserId == userId && x.Date == date, ct);
    }

    public void AddDailyStats(DailyStats dailyStats)
    {
        _db.DailyStats.Add(dailyStats);
    }

    public Task<bool> HasRecommendationAsync(
        string userId,
        RecommendationType type,
        string target,
        RecommendationStatus status,
        CancellationToken ct)
    {
        return _db.Recommendations.AnyAsync(
            x => x.UserId == userId && x.Type == type && x.Target == target && x.Status == status,
            ct);
    }

    public void AddRecommendation(Recommendation recommendation)
    {
        _db.Recommendations.Add(recommendation);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}
