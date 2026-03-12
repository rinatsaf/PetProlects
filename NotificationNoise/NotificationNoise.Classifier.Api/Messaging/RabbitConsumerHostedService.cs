using NotificationNoise.BuildingBlocks;
using NotificationNoise.BuildingBlocks.Messaging;

namespace NotificationNoise.Classifier.Api.Messaging;

public sealed class RabbitConsumerHostedService : BackgroundService
{
    private readonly IEventConsumer _consumer;

    public RabbitConsumerHostedService(IEventConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => _consumer.StartAsync(stoppingToken);
}