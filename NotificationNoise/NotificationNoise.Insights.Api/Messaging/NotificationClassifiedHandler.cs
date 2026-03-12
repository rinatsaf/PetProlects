using System.Text.Json;
using Contracts.Notifications;
using NotificationNoise.Insights.Application;

namespace NotificationNoise.Insights.Api.Messaging;

public sealed class NotificationClassifiedHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<NotificationClassifiedHandler> _logger;

    public NotificationClassifiedHandler(IServiceScopeFactory serviceScopeFactory, ILogger<NotificationClassifiedHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(string routingKey, string json, CancellationToken ct)
    {
        if (routingKey != "notification.classified")
        {
            return;
        }

        NotificationClassified? notification;
        try
        {
            notification = JsonSerializer.Deserialize<NotificationClassified>(json);
            if (notification is null)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize NotificationClassified");
            return;
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<NotificationInsightsService>();
            await service.ApplyAsync(notification, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle message");
        }
    }
}
