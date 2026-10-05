namespace Application.DTOs.Tickets;

public sealed class TicketDto
{
    public long Id { get; set; }
    public long? OrderId { get; set; }
    public long SessionId { get; set; }
    public long SeatId { get; set; }
    public decimal Price { get; set; }
    public string TicketCode { get; set; } = null!;
    public string? QrCodeUrl { get; set; }
    public string Status { get; set; } = null!;
}
