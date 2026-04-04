namespace Application.DTOs.Sessions;

public sealed class SessionDto
{
    public long Id { get; set; }
    public long MovieId { get; set; }
    public long HallId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = null!;
}
