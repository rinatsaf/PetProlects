namespace Application.DTOs.Orders;

public sealed class CreateOrderRequest
{
    public long? UserId { get; set; }
    public long SessionId { get; set; }
    public IEnumerable<long> SeatIds { get; set; } = Array.Empty<long>();
}
