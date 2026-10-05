using Application.Abstractions.Services;

namespace Api.Hangfire.Jobs;

public sealed class SessionLifecycleJob(
    ISessionLifecycleService lifecycleService,
    ILogger<SessionLifecycleJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var (activated, finished) = await lifecycleService.AdvanceStatusesAsync(DateTimeOffset.UtcNow, cancellationToken);

        if (activated > 0 || finished > 0)
        {
            logger.LogInformation(
                "Session lifecycle updated. Activated: {Activated}, Finished: {Finished}",
                activated,
                finished);
        }
    }
}
