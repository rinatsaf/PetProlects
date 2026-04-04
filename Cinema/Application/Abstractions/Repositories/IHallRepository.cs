using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IHallRepository
{
    Task<IReadOnlyList<Hall>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Hall?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, long excludeId, CancellationToken cancellationToken = default);
    Task<Hall> AddAsync(Hall hall, CancellationToken cancellationToken = default);
    Task<Hall> UpdateAsync(Hall hall, CancellationToken cancellationToken = default);
    Task<Hall> DeleteAsync(Hall hall, CancellationToken cancellationToken = default);
}
