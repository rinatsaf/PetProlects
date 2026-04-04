using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByExternalIdAsync(string externalPaymentId, CancellationToken cancellationToken = default);
    Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}
