using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserAsync(long userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetExpiredPendingAsync(DateTimeOffset now, int take, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order> AddWithTicketsAsync(Order order, IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
