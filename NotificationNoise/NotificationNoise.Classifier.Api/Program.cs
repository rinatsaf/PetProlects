using NotificationNoise.BuildingBlocks;
using NotificationNoise.BuildingBlocks.Messaging;
using NotificationNoise.BuildingBlocks.Messaging.RabbitMq;
using NotificationNoise.Classifier.Api.Messaging;
using NotificationNoise.Classifier.Api.Services;
using NotificationNoise.Classifier.Application;
using NotificationNoise.Classifier.Application.Rules;
using NotificationNoise.Classifier.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddClassifierInfrastructure(builder.Configuration);

var rabbit = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddSingleton<IEventConsumer>(sp =>
{
    var handler = sp.GetRequiredService<NotificationReceivedHandler>();
    var lifetime = sp.GetRequiredService<IHostApplicationLifetime>();

    return new RabbitMqConsumer(
        host: rabbit["Host"]!,
        port: int.Parse(rabbit["Port"]!),
        user: rabbit["User"]!,
        pass: rabbit["Pass"]!,
        queue: "nn.classifier.notification-received",
        bindingKey: "notification.received",
        onMessage: (rk, json) =>
        {
            _ = Task.Run(() => handler.HandleAsync(rk, json, lifetime.ApplicationStopping));
        });
});
builder.Services.AddSingleton<IEventPublisher>(_ =>
    new RabbitMqPublisher(
        rabbit["Host"]!,
        int.Parse(rabbit["Port"]!),
        rabbit["User"]!,
        rabbit["Pass"]!));

builder.Services.AddSingleton<NotificationReceivedHandler>();

builder.Services.AddHostedService<RabbitConsumerHostedService>();

builder.Services.AddSingleton<IRulesEngine, SimpleRulesEngine>();
builder.Services.AddScoped<IClassificationResultPublisher, NotificationClassifiedPublisher>();
builder.Services.AddScoped<NotificationClassificationService>();

var app = builder.Build();
app.MapControllers();

app.Run();
