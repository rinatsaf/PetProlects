namespace Infrastructure.Options;

public sealed class LoginRateLimitOptions
{
    public const string SectionName = "RateLimiting:Login";

    public int EmailThreshold { get; set; } = 5;
    public int WindowMinutes { get; set; } = 10;
}
