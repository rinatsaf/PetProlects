using Domain.Enums;

namespace Application.DTOs.Payments;

public sealed class PaymentStatusUpdateRequest
{
    public PaymentStatus Status { get; set; }
    public string? ConfirmationUrl { get; set; }
    public string? RawPayload { get; set; }
}
