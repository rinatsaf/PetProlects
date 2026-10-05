namespace Infrastructure.Options;

public sealed class PreferenceCleanupOptions
{
    public const string SectionName = "PreferenceCleanup";
    public decimal DecayFactor { get; set; } = 0.75m;
    public decimal DeleteThreshold { get; set; } = 0.10m;
}
