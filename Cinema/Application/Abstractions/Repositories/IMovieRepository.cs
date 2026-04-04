using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMovieRepository
{
    Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Movie> AddAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<Movie> UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<Movie> DeleteAsync(Movie movie, CancellationToken cancellationToken = default);
}