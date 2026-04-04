namespace Application.DTOs.Tickets;

public sealed class IssueTicketsRequest
{
    public long OrderId { get; set; }
    public long SessionId { get; set; }
    public IEnumerable<SeatTicketItem> Seats { get; set; } = Array.Empty<SeatTicketItem>();
    public decimal Price { get; set; }
}

public sealed class SeatTicketItem
{
    public long SeatId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string? QrCodeUrl { get; set; }
}
