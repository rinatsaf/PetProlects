using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationNoise.Insights.Application;
using NotificationNoise.Insights.Infrastructure.Persistence;

namespace NotificationNoise.Insights.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInsightsInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("Db")
                 ?? throw new InvalidOperationException();

        services.AddDbContext<InsightsDbContext>(o => o.UseNpgsql(cs));
        services.AddScoped<IInsightsRepository, InsightsRepository>();
        return services;
    }
}
