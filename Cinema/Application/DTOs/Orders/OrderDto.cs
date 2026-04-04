namespace Application.DTOs.Orders;

public sealed class OrderDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
