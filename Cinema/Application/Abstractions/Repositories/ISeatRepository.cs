using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ISeatRepository
{
    Task<IReadOnlyList<Seat>> GetByHallAsync(long hallId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Seat>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
    Task<Seat?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPositionAsync(long hallId, int rowNumber, int seatNumber, CancellationToken cancellationToken = default);
    Task<Seat> AddAsync(Seat seat, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default);
    Task<Seat> UpdateAsync(Seat seat, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default);
}
