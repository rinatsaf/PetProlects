namespace NotificationNoise.BuildingBlocks;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T message, CancellationToken ct = default);
}