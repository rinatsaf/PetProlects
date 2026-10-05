using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Session : BaseEntity
{
    public long CreatedByUserId { get; set; }
    public long MovieId { get; set; }
    public required Movie Movie { get; set; }

    public long HallId { get; set; }
    public required Hall Hall { get; set; }

    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Planned;
    
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
