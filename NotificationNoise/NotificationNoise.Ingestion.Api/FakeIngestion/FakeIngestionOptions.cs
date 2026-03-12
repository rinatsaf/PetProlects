namespace NotificationNoise.Ingestion.Api.FakeIngestion;

public sealed class FakeIngestionOptions
{
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 10;
}