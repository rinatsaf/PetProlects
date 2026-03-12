using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationNoise.BuildingBlocks.Messaging.RabbitMq;

public class RabbitMqConsumer : IEventConsumer, IDisposable
{
    private readonly IConnection _conn;
    private readonly IModel _ch;
    private readonly string _queue;

    public RabbitMqConsumer(
        string host,
        int port,
        string user,
        string pass,
        string queue,
        string bindingKey,
        Action<string, string> onMessage)
    {
        var factory = new ConnectionFactory()
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass,
        };
        _conn = factory.CreateConnection();
        _ch = _conn.CreateModel();
        
        _ch.ExchangeDeclare("nn.events", ExchangeType.Topic, durable:true, autoDelete:false);
        
        _queue = queue;
        _ch.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false);
        _ch.QueueBind(_queue, "nn.events", bindingKey);
        
        var consumer = new EventingBasicConsumer(_ch);
        consumer.Received += (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            onMessage(ea.RoutingKey, body);
            _ch.BasicAck(ea.DeliveryTag, false);
        };

        _ch.BasicConsume(queue: _queue, autoAck: false, consumer: consumer);
    }
    
    public Task StartAsync(CancellationToken ct = default)
    {
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