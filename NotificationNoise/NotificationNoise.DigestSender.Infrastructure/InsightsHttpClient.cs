using System.Net.Http.Json;
using NotificationNoise.DigestSender.Application;

namespace NotificationNoise.DigestSender.Infrastructure;

public sealed class InsightsHttpClient : IInsightsClient
{
    private readonly HttpClient _http;

    public InsightsHttpClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<DigestSnapshot> GetSnapshotAsync(string userId, int days, int top, CancellationToken ct)
    {
        var topSenders = await _http
            .GetFromJsonAsync<List<TopSenderItem>>($"/stats/top-senders?limit={top}", ct)
            ?? new List<TopSenderItem>();

        var trends = await _http
            .GetFromJsonAsync<List<TrendItem>>($"/stats/trends?days={days}", ct)
            ?? new List<TrendItem>();

        var recommendations = await _http
            .GetFromJsonAsync<List<RecommendationItem>>("/recommendations?status=new", ct)
            ?? new List<RecommendationItem>();

        return new DigestSnapshot(
            UserId: userId,
            Days: days,
            Top: top,
            TopSenders: topSenders,
            Trends: trends,
            Recommendations: recommendations
        );
    }
}
