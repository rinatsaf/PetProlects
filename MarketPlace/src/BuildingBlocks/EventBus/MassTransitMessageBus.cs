using MassTransit;

namespace EventBus;

public sealed class MassTransitMessageBus(IBus bus) : IMessageBus
{
    public async Task PublishAsync<T>(T @event, CancellationToken token = default) where T : IntegrationEvent
    {
        await bus.Publish(@event, token);
    }
}