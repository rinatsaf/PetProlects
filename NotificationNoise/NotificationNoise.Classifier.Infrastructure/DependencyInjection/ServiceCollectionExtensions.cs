using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationNoise.Classifier.Application;
using NotificationNoise.Classifier.Infrastructure.Persistence;

namespace NotificationNoise.Classifier.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClassifierInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("Db") 
                 ?? throw new InvalidOperationException("Db connection string not found");
        
        services.AddDbContext<ClassifierDbContext>(o => o.UseNpgsql(cs));
        services.AddScoped<IClassificationRepository, ClassificationRepository>();
        return services;
    }
}
