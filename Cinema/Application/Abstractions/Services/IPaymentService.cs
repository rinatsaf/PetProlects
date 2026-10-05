using Application.DTOs.Payments;

namespace Application.Abstractions.Services;

public interface IPaymentService
{
    Task<PaymentDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentDto>> GetAccessibleAsync(CancellationToken cancellationToken = default);
    Task<PaymentDto> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<PaymentDto> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDto> UpdateStatusAsync(long id, PaymentStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDto> HandleYooKassaWebhookAsync(YooKassaWebhookRequest request, string rawPayload, CancellationToken cancellationToken = default);
    Task<int> SynchronizePendingPaymentsAsync(CancellationToken cancellationToken = default);
}
