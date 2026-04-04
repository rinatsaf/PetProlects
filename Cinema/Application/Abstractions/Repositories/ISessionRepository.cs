using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ISessionRepository
{
    Task<IReadOnlyList<Session>> GetAllUpcomingAsync(CancellationToken cancellationToken = default);
    Task<Session?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session> UpdateAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session> DeleteAsync(Session session, CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(long hallId, DateTimeOffset startTime, DateTimeOffset endTime, long? excludeId = null, CancellationToken cancellationToken = default);
}
