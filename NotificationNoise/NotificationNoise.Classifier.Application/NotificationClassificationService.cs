using System.Text.Json;
using Contracts.Notifications;
using NotificationNoise.Classifier.Application.Rules;
using NotificationNoise.Classifier.Domain;

namespace NotificationNoise.Classifier.Application;

public sealed class NotificationClassificationService
{
    private readonly IClassificationRepository _repository;
    private readonly IClassificationResultPublisher _publisher;
    private readonly IRulesEngine _rulesEngine;

    public NotificationClassificationService(
        IClassificationRepository repository,
        IClassificationResultPublisher publisher,
        IRulesEngine rulesEngine)
    {
        _repository = repository;
        _publisher = publisher;
        _rulesEngine = rulesEngine;
    }

    public Task<int> GetCountAsync(CancellationToken ct)
    {
        return _repository.CountAsync(ct);
    }

    public async Task ProcessAsync(NotificationReceived notification, CancellationToken ct)
    {
        var exists = await _repository.ExistsAsync(
            notification.UserId,
            notification.Provider,
            notification.ExternalMessageId,
            ct);

        if (exists)
        {
            return;
        }

        var result = _rulesEngine.Evaluate(notification);
        var classifiedAt = DateTimeOffset.UtcNow;
        var classification = new Classification(
            notificationId: notification.NotificationId,
            userId: notification.UserId,
            provider: notification.Provider,
            externalMessageId: notification.ExternalMessageId,
            label: result.Label,
            score: result.Score,
            reasonsJson: JsonSerializer.Serialize(result.Reasons),
            classifiedAt: classifiedAt);

        _repository.Add(classification);
        await _repository.SaveChangesAsync(ct);

        var message = new NotificationClassified(
            NotificationId: notification.NotificationId,
            UserId: notification.UserId,
            Provider: notification.Provider,
            ExternalMessageId: notification.ExternalMessageId,
            FromEmail: notification.FromEmail,
            FromName: notification.FromName,
            ReceivedAt: notification.ReceivedAt,
            HasListUnsubscribe: notification.HasListUnsubscribe,
            Label: result.Label,
            Score: result.Score,
            Reasons: result.Reasons,
            ClassifiedAt: classifiedAt);

        await _publisher.PublishAsync(message, ct);
    }
}
