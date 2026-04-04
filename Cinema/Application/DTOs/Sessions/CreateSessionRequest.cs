using Domain.Enums;

namespace Application.DTOs.Sessions;

public sealed class CreateSessionRequest
{
    public long MovieId { get; set; }
    public long HallId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Planned;
}
