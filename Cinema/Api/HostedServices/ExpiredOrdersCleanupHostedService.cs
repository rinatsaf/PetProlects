using Application.Abstractions.Services;

namespace Api.HostedServices;

public sealed class ExpiredOrdersCleanupHostedService(IServiceScopeFactory scopeFactory, ILogger<ExpiredOrdersCleanupHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromMinutes(1);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                // Hosted Sevice у нас синглтон, поэтому достаем завимсоть так
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
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }
    }
}
