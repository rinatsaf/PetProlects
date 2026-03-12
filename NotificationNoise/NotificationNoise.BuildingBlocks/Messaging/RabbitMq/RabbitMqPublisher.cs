using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace NotificationNoise.BuildingBlocks.Messaging.RabbitMq;

public sealed class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _conn;
    private readonly IModel _ch;

    public RabbitMqPublisher(string host, int port, string user, string pass)
    {
        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass
        };

        _conn = factory.CreateConnection();
        _ch = _conn.CreateModel();

        _ch.ExchangeDeclare("nn.events", ExchangeType.Topic, durable: true, autoDelete: false);
    }

    public Task PublishAsync<T>(string topic, T message, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _ch.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2;

        _ch.BasicPublish("nn.events", topic, props, body);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        try { _ch.Close(); } catch { }
        try { _conn.Close(); } catch { }
        _ch.Dispose();
        _conn.Dispose();
    }
}