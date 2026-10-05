using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IGenreRepository
{
    Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Genre?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, long excludeId, CancellationToken cancellationToken = default);

    Task<Genre> AddAsync(Genre genre, CancellationToken cancellationToken = default);
    Task<Genre> UpdateAsync(Genre genre, CancellationToken cancellationToken = default);
    Task<Genre> DeleteAsync(Genre genre, CancellationToken cancellationToken = default);
}
