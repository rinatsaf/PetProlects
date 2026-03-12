using System.Text.Json;
using Contracts.Notifications;
using NotificationNoise.Classifier.Application;

namespace NotificationNoise.Classifier.Api.Messaging;

public sealed class NotificationReceivedHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationReceivedHandler> _logger;

    public NotificationReceivedHandler(IServiceScopeFactory scopeFactory, ILogger<NotificationReceivedHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(string routingKey, string json, CancellationToken ct)
    {
        if (routingKey != "notification.received")
        {
            return;
        }

        NotificationReceived? notification;
        try
        {
            notification = JsonSerializer.Deserialize<NotificationReceived>(json);
            if (notification is null)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize NotificationReceived");
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<NotificationClassificationService>();
            await service.ProcessAsync(notification, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle message");
        }
    }
}
