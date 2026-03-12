using Contracts.Notifications;
using NotificationNoise.BuildingBlocks;
using NotificationNoise.Classifier.Application;

namespace NotificationNoise.Classifier.Api.Services;

public sealed class NotificationClassifiedPublisher : IClassificationResultPublisher
{
    private readonly IEventPublisher _publisher;

    public NotificationClassifiedPublisher(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task PublishAsync(NotificationClassified message, CancellationToken ct)
    {
        return _publisher.PublishAsync("notification.classified", message, ct);
    }
}
