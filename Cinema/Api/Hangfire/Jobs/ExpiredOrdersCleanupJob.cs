using Application.Abstractions.Services;

namespace Api.Hangfire.Jobs;

public sealed class ExpiredOrdersCleanupJob(
    IOrderCleanupService cleanupService,
    ILogger<ExpiredOrdersCleanupJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var count = await cleanupService.CancelExpiredAsync(cancellationToken);

        if (count > 0)
        {
            logger.LogInformation("Expired orders cancelled: {Count}", count);
        }
    }
}