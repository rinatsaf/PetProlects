using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationNoise.Ingestion.Application;
using NotificationNoise.Ingestion.Infrastructure;
using NotificationNoise.Ingestion.Infrastructure.Persistence;

namespace NotificationNoise.Ingestion.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIngestionInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("Db")
                 ?? throw new InvalidOperationException("Connection string not found");
        services.AddDbContext<IngestionDbContext>(o => o.UseNpgsql(cs));

        services.AddSingleton<ITokenProtector, TokenProtector>();
        services.AddScoped<IGmailTokenStore, GmailTokenStore>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddHttpClient<IGoogleOAuthClient, GoogleOAuthClient>(c =>
        {
            c.BaseAddress = new Uri("https://oauth2.googleapis.com");
        });

        services.AddHttpClient<IGmailMessageClient, GmailApiClient>(c =>
        {
            c.BaseAddress = new Uri("https://gmail.googleapis.com/gmail/v1/users/me/");
        });

        return services;
    }
}
