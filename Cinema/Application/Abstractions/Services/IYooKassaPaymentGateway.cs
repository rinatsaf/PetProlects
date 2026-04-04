using Application.DTOs.Payments;

namespace Application.Abstractions.Services;

public interface IYooKassaPaymentGateway
{
    Task<YooKassaCreatePaymentResult> CreatePaymentAsync(
        long orderId,
        decimal amount,
        string currency,
        string returnUrl,
        CancellationToken cancellationToken = default);
}
