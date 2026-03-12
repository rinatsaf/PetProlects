using Contracts.Notifications;

namespace NotificationNoise.Classifier.Application;

public interface IClassificationResultPublisher
{
    Task PublishAsync(NotificationClassified message, CancellationToken ct);
}
