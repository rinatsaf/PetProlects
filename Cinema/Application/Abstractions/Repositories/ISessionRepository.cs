using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ISessionRepository
{
    Task<IReadOnlyList<Session>> GetAllUpcomingAsync(CancellationToken cancellationToken = default);
    Task<Session?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session> AddWithTicketsAsync(Session session, IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
    Task<Session> UpdateAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session> DeleteAsync(Session session, CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(long hallId, DateTimeOffset startTime, DateTimeOffset endTime, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> ActivatePlannedAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<int> FinishActiveAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Session>> GetUpcomingByHallAsync(long hallId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Session>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
}
