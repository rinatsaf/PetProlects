using Microsoft.AspNetCore.DataProtection;
using NotificationNoise.BuildingBlocks;
using NotificationNoise.BuildingBlocks.Messaging.RabbitMq;
using NotificationNoise.Ingestion.Api;
using NotificationNoise.Ingestion.Api.FakeIngestion;
using NotificationNoise.Ingestion.Api.Services;
using NotificationNoise.Ingestion.Application;
using NotificationNoise.Ingestion.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddIngestionInfrastructure(builder.Configuration);
var keysPath = builder.Configuration["DataProtection:KeysPath"];
var dp = builder.Services.AddDataProtection();
if (!string.IsNullOrWhiteSpace(keysPath))
{
    dp.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

builder.Services.AddSingleton(sp =>
{
    var cfg = builder.Configuration.GetSection("Gmail");
    var scopes = cfg.GetSection("Scopes").Get<string[]>() ??
                 new[] { "https://www.googleapis.com/auth/gmail.readonly" };
    return new OAuthSettings(
        ClientId: cfg["ClientId"] ?? "",
        ClientSecret: cfg["ClientSecret"] ?? "",
        RedirectUri: cfg["RedirectUri"] ?? "",
        Scopes: scopes);
});

builder.Services.AddScoped<GmailOAuthService>();
builder.Services.AddScoped<GmailBackfillService>();
builder.Services.AddScoped<NotificationDebugService>();
builder.Services.AddScoped<INotificationReceivedPublisher, NotificationReceivedPublisher>();

builder.Services.Configure<FakeIngestionOptions>(builder.Configuration.GetSection("FakeIngestion"));
builder.Services.AddHostedService<FakeIngestionHostedService>();

var rabbit = builder.Configuration.GetSection("RabbitMq");
builder.Services.AddSingleton<IEventPublisher>(_ =>
    new RabbitMqPublisher(
        rabbit["Host"]!,
        int.Parse(rabbit["Port"]!),
        rabbit["User"]!,
        rabbit["Pass"]!));

var app = builder.Build();
app.MapControllers();

app.Run();
