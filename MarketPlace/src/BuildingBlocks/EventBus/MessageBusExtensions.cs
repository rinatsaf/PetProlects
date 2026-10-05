using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus;

public static class MessageBusExtensions
{
    public static IServiceCollection AddMessageBus(
        this IServiceCollection services,
        string rabbitMqConnection = "localhost")
    {
        services.AddMassTransit(config =>
        {
            config.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitMqConnection, "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.AddScoped<IMessageBus, MassTransitMessageBus>();

        return services;
    }
}