namespace Application.DTOs.Sessions;

public sealed class SessionScheduleDto
{
    public long SessionId { get; set; }
    public string MovieTitle { get; set; } = null!;
    public string? PosterUrl { get; set; }
    public string HallName { get; set; } = null!;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public int AvailableSeats { get; set; }
}
