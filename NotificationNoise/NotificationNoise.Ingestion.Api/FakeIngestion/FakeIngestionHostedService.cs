using Contracts.Notifications;
using NotificationNoise.BuildingBlocks.Messaging;
using Microsoft.Extensions.Options;
using NotificationNoise.BuildingBlocks;

namespace NotificationNoise.Ingestion.Api.FakeIngestion;

public sealed class FakeIngestionHostedService : BackgroundService
{
    private readonly IEventPublisher _bus;
    private readonly FakeIngestionOptions _opt;
    private readonly ILogger<FakeIngestionHostedService> _logger;
    private readonly Random _rnd = new();

    private static readonly (string Email, string Name)[] Senders =
    {
        ("news@service.com", "Service News"),
        ("no-reply@github.com", "GitHub"),
        ("offers@shop.com", "Shop"),
        ("alerts@bank.com", "Bank"),
        ("team@product.com", "Product Team"),
        ("updates@company.com", "Company Updates"),
    };

    public FakeIngestionHostedService(
        IEventPublisher bus,
        IOptions<FakeIngestionOptions> opt,
        ILogger<FakeIngestionHostedService> logger)
    {
        _bus = bus;
        _opt = opt.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opt.Enabled)
        {
            _logger.LogInformation("FakeIngestion disabled.");
            return;
        }

        _logger.LogInformation("FakeIngestion started: every {Sec}s batch={Batch}",
            _opt.IntervalSeconds, _opt.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            for (var i = 0; i < _opt.BatchSize; i++)
            {
                var s = Senders[_rnd.Next(Senders.Length)];

                var ev = new NotificationReceived(
                    NotificationId: Guid.NewGuid(),
                    UserId: "demo",
                    Provider: "gmail",
                    ExternalMessageId: Guid.NewGuid().ToString("N"),
                    FromEmail: s.Email,
                    FromName: s.Name,
                    Subject: $"Auto generated #{_rnd.Next(10000)}",
                    Snippet: "Fake ingestion event",
                    ReceivedAt: DateTimeOffset.UtcNow.AddMinutes(-_rnd.Next(0, 60 * 24 * 7)),
                    HasListUnsubscribe: _rnd.Next(0, 2) == 1
                );

                await _bus.PublishAsync("notification.received", ev, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(_opt.IntervalSeconds), stoppingToken);
        }
    }
}
