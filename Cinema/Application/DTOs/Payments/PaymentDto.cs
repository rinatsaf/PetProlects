namespace Application.DTOs.Payments;

public sealed class PaymentDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string Provider { get; set; } = null!;
    public string ExternalPaymentId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RUB";
    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? ConfirmationUrl { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
