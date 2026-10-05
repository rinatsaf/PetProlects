using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(long id, CurrentUserInfo info, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByCodeAsync(string code, CurrentUserInfo info, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetByOrderAsync(long orderId, CurrentUserInfo info, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetAvailableBySessionAsync(long sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetAvailableBySessionAndSeatsAsync(long sessionId, IEnumerable<long> seatIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> GetBlockingTicketsAsync(long sessionId, IEnumerable<long> seatIds, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
    Task<Ticket> UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
}
