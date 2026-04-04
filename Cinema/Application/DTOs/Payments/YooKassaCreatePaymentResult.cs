using Domain.Enums;

namespace Application.DTOs.Payments;

public sealed class YooKassaCreatePaymentResult
{
    public string ExternalPaymentId { get; init; } = string.Empty;
    public PaymentStatus Status { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string? ConfirmationUrl { get; init; }
    public string RawPayload { get; init; } = string.Empty;
}
