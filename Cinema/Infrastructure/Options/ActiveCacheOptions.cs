namespace Infrastructure.Options;

public sealed class ActiveCacheOptions
{
    public const string SectionName = "ActiveCache";
    public int TtlSeconds { get; set; } = 60;
}
