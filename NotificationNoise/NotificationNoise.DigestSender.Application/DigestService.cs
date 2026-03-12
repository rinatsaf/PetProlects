using System.Text.Json;
using NotificationNoise.DigestSender.Domain;

namespace NotificationNoise.DigestSender.Application;

public sealed class DigestService
{
    private readonly IInsightsClient _insights;
    private readonly IDigestRepository _repo;
    private readonly IDigestDelivery _delivery;

    public DigestService(IInsightsClient insights, IDigestRepository repo, IDigestDelivery delivery)
    {
        _insights = insights;
        _repo = repo;
        _delivery = delivery;
    }

    public async Task<Digest> RunAsync(string userId, string period, int days, int top, CancellationToken ct)
    {
        var snapshot = await _insights.GetSnapshotAsync(userId, days, top, ct);
        var payloadJson = JsonSerializer.Serialize(snapshot);

        var digest = new Digest(
            userId: userId,
            period: period,
            payloadJson: payloadJson,
            createdAt: DateTimeOffset.UtcNow);

        await _repo.AddAsync(digest, ct);
        await _repo.SaveChangesAsync(ct);

        await _delivery.SendAsync(userId, period, payloadJson, ct);
        digest.MarkSent(DateTimeOffset.UtcNow);

        await _repo.SaveChangesAsync(ct);
        return digest;
    }
}
