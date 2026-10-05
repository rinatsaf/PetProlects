namespace Application.DTOs.Orders;

public sealed class OrderDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public int TicketsCount { get; set; }
    public IEnumerable<long> TicketIds { get; set; } = Array.Empty<long>();
    public IEnumerable<long> PaymentIds { get; set; } = Array.Empty<long>();
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
