namespace EventBus;

public interface IMessageBus
{
    Task PublishAsync<T>(T @event, CancellationToken token = default) 
        where T : IntegrationEvent;
}