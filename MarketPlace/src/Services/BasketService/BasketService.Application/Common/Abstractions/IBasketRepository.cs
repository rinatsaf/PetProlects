using BasketService.Domain;

namespace BasketService.Application.Common.Abstractions;

public interface IBasketRepository
{
    Task<Basket?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task UpsertAsync(Basket basket, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default);
}