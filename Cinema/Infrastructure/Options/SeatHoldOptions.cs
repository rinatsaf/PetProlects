namespace Infrastructure.Options;

public sealed class SeatHoldOptions
{
    public const string SectionName = "SeatHold";
    public int TtlMinutes { get; set; } = 10;
}
