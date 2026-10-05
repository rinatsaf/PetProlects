using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Payment : BaseEntity
{
    public long CreatedByUserId { get; set; }
    public long OrderId { get; set; }
    public required Order Order { get; set; } 
    
    public string Provider { get; set; } = "YooKassa";
    public required string ExternalPaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RUB";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string PaymentMethod { get; set; } = "SBP";
    public string? ConfirmationUrl { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public string? RawPayload { get; set; }
}
