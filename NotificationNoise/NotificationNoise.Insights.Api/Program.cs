using NotificationNoise.BuildingBlocks;
using NotificationNoise.BuildingBlocks.Messaging.RabbitMq;
using NotificationNoise.Insights.Api.Messaging;
using NotificationNoise.Insights.Application;
using NotificationNoise.Insights.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInsightsInfrastructure(builder.Configuration);

var rabbit = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddSingleton<IEventConsumer>(sp =>
{
    var handler = sp.GetRequiredService<NotificationClassifiedHandler>();
    var lifetime = sp.GetRequiredService<IHostApplicationLifetime>();

    return new RabbitMqConsumer(
        host: rabbit["Host"]!,
        port: int.Parse(rabbit["Port"]!),
        user: rabbit["User"]!,
        pass: rabbit["Pass"]!,
        queue: "nn.insights.notification-classified",
        bindingKey: "notification.classified",
        onMessage: (rk, json) =>
        {
            _ = Task.Run(() => handler.HandleAsync(rk, json, lifetime.ApplicationStopping));
        });
});

builder.Services.AddSingleton<NotificationClassifiedHandler>();
builder.Services.AddHostedService<RabbitConsumerHostedService>();
builder.Services.AddScoped<InsightsQueryService>();
builder.Services.AddScoped<NotificationInsightsService>();

var app = builder.Build();
app.MapControllers();

app.Run();
