using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Payments;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CinemaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));

        services.AddOptions<YooKassaOptions>()
            .Bind(configuration.GetSection(YooKassaOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.ShopId) &&
                !string.IsNullOrWhiteSpace(options.SecretKey),
                "YooKassa credentials are not configured.")
            .ValidateOnStart();

        services.AddHttpClient<IYooKassaPaymentGateway, YooKassaPaymentGateway>(client =>
        {
            client.BaseAddress = new Uri("https://api.yookassa.ru/v3/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserInteractionRepository, UserInteractionRepository>();
        services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
        services.AddScoped<ISeatHoldService, SeatHoldService>();
        services.AddScoped<IOrderCleanupService, OrderCleanupService>();

        services.AddSecurity();
    }
}
