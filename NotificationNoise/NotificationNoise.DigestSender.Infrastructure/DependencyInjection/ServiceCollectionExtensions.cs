using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationNoise.DigestSender.Application;
using NotificationNoise.DigestSender.Infrastructure;
using NotificationNoise.DigestSender.Infrastructure.Persistence;

namespace NotificationNoise.DigestSender.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDigestSenderInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("Db") 
                 ?? throw new InvalidOperationException();

        services.AddDbContext<DigestDbContext>(o => o.UseNpgsql(cs));
        services.AddScoped<IDigestRepository, DigestRepository>();
        services.AddSingleton<IDigestDelivery, LogDigestDelivery>();

        var insightsBaseUrl = configuration["Insights:BaseUrl"] ?? "http://localhost:5003";
        services.AddHttpClient<IInsightsClient, InsightsHttpClient>(c =>
        {
            c.BaseAddress = new Uri(insightsBaseUrl);
        });

        return services;
    }
}
