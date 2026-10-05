using Api.Hangfire.Jobs;
using Hangfire;

namespace Api.Hangfire;

public static class RecurringJobsRegistrar
{
    public static void Register(IRecurringJobManager jobs)
    {
        jobs.AddOrUpdate<ExpiredOrdersCleanupJob>(
            "expired-orders-cleanup",
            job => job.RunAsync(CancellationToken.None),
            Cron.Minutely);

        jobs.AddOrUpdate<SessionLifecycleJob>(
            "session-lifecycle",
            job => job.RunAsync(CancellationToken.None),
            Cron.Minutely);

        jobs.AddOrUpdate<PreferenceDecayJob>(
            "preferences-weekly-decay",
            job => job.RunAsync(CancellationToken.None),
            Cron.Weekly);

        jobs.AddOrUpdate<PaymentConfirmationJob>(
            "payment-confirmation",
            job => job.RunAsync(CancellationToken.None),
            "*/15 * * * * *");
    }
}
