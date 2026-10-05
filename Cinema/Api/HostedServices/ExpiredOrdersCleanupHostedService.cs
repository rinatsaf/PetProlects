using Application.Abstractions.Services;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Api.HostedServices;

public sealed class ExpiredOrdersCleanupHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredOrdersCleanupHostedService> logger,
    IOptionsSnapshot<OrderCleanupOptions> options) : BackgroundService
{
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(options.Value.PollingIntervalSec);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<IOrderCleanupService>();
                var count = await cleanupService.CancelExpiredAsync(stoppingToken);
                if (count > 0)
                {
                    logger.LogInformation("Expired orders cancelled: {Count}", count);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while cancelling expired orders");
            }

            try
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (TaskCanceledException ex)
            {
                throw new OperationCanceledException("Expired orders cleanup task was cancelled", ex, stoppingToken);
            }
        }
    }
}
