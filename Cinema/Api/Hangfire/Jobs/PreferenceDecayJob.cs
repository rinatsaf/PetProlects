using Application.Abstractions.Services;

namespace Api.Hangfire.Jobs;

public sealed class PreferenceDecayJob(
    IPreferenceCleanupService cleanupService,
    ILogger<PreferenceDecayJob> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var affected = await cleanupService.ApplyWeeklyDecayAsync(cancellationToken);
        if (affected > 0)
        {
            logger.LogInformation("User preferences weekly decay applied. Affected rows: {Affected}", affected);
        }
    }
}
