namespace NotificationNoise.BuildingBlocks;

public interface IEventConsumer
{
    Task StartAsync(CancellationToken ct = default);
}