namespace Infrastructure.Options;

public sealed class OrderCleanupOptions
{
    public const string SectionName = "OrderCleanup";
    public int BatchSize { get; set; } = 100;
    public int PollingIntervalSec { get; set; } = 60;
}
