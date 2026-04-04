using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetByOrderAsync(long orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetBlockingTicketsAsync(long sessionId, IEnumerable<long> seatIds, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
    Task<Ticket> UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
}
