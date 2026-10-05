namespace Application.DTOs.Sessions;

public sealed class SessionDto
{
    public long Id { get; set; }
    public long CreatedByUserId { get; set; }
    public long MovieId { get; set; }
    public string MovieTitle { get; set; } = null!;
    public long HallId { get; set; }
    public string HallName { get; set; } = null!;
    public string HallAddress { get; set; } = null!;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = null!;
}
