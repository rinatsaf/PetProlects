using Application.DTOs.Payments;
using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IYooKassaPaymentGateway
{
    Task<YooKassaCreatePaymentResult> CreatePaymentAsync(
        long orderId,
        decimal amount,
        string currency,
        string returnUrl,
        CancellationToken cancellationToken = default);

    Task<PaymentStatus> GetPaymentStatusAsync(
        string externalPaymentId,
        CancellationToken cancellationToken = default);

    Task<YooKassaRefundResult> CreateRefundAsync(string externalPaymentId, decimal amount, string currency,
        string idempotencyKey, CancellationToken ct);
}
