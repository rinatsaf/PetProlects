using Api.Hangfire.Jobs;
using Hangfire;
using Hangfire.PostgreSql;

namespace Api.Hangfire;

public static class DependencyInjection
{
    public static IServiceCollection AddCinemaHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ExpiredOrdersCleanupJob>();
        services.AddScoped<SessionLifecycleJob>();
        services.AddScoped<PreferenceDecayJob>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(
                    configuration.GetConnectionString("HangfireConnection"))));

        services.AddHangfireServer(options =>
        {
            options.CancellationCheckInterval = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
