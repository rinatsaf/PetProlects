using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Ticket : BaseEntity
{ 
    public long OrderId { get; set; }
    public Order Order { get; set; }

    public long SessionId { get; set; }
    public Session Session { get; set; }

    public long SeatId { get; set; }
    public Seat Seat { get; set; }

    public decimal Price { get; set; }
    public required string TicketCode { get; set; }
    public string? QrCodeUrl { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Active;
}