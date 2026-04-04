namespace Application.DTOs.Payments;

public sealed class CreatePaymentRequest
{
    public long OrderId { get; set; }
    public string ReturnUrl { get; set; } = null!;
}
