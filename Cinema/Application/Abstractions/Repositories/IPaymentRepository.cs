using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(long id, CurrentUserInfo info, CancellationToken cancellationToken = default);
    Task<Payment?> GetByExternalIdAsync(string externalPaymentId, CurrentUserInfo info, CancellationToken cancellationToken = default);
    Task<Payment?> GetByExternalIdUnsafeAsync(string externalPaymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetAccessibleAsync(CurrentUserInfo info, CancellationToken cancellationToken = default);
    Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}
