using Contracts.Notifications;
using NotificationNoise.Insights.Domain;

namespace NotificationNoise.Insights.Application;

public sealed class NotificationInsightsService
{
    private readonly IInsightsRepository _repository;

    public NotificationInsightsService(IInsightsRepository repository)
    {
        _repository = repository;
    }

    public async Task ApplyAsync(NotificationClassified notification, CancellationToken ct)
    {
        var isNoise = notification.Label.Equals("noise", StringComparison.OrdinalIgnoreCase);
        var senderKey = notification.FromEmail.ToLowerInvariant();
        var date = notification.ReceivedAt.UtcDateTime.Date;

        var sender = await _repository.GetSenderStatsAsync(notification.UserId, senderKey, ct);
        if (sender is null)
        {
            sender = new SenderStats(notification.UserId, senderKey, notification.ReceivedAt, isNoise);
            _repository.AddSenderStats(sender);
        }
        else
        {
            sender.ApplyNotification(isNoise, notification.ReceivedAt);
        }

        var daily = await _repository.GetDailyStatsAsync(notification.UserId, date, ct);
        if (daily is null)
        {
            daily = new DailyStats(notification.UserId, date, isNoise);
            _repository.AddDailyStats(daily);
        }
        else
        {
            daily.ApplyNotification(isNoise);
        }

        if (isNoise && notification.HasListUnsubscribe)
        {
            var exists = await _repository.HasRecommendationAsync(
                notification.UserId,
                RecommendationType.Unsubscribe,
                senderKey,
                RecommendationStatus.New,
                ct);

            if (!exists)
            {
                var impact = Math.Min(100, sender.NoiseCount * 5);
                _repository.AddRecommendation(new Recommendation(
                    userId: notification.UserId,
                    type: RecommendationType.Unsubscribe,
                    target: senderKey,
                    why: "many_noise_emails_with_list_unsubscribe",
                    impactScore: impact,
                    createdAt: DateTimeOffset.UtcNow));
            }
        }

        await _repository.SaveChangesAsync(ct);
    }
}
