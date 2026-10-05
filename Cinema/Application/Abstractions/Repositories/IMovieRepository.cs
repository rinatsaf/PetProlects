using Application.DTOs.Movies;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMovieRepository
{
    Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Movie>> SearchAsync(MovieSearchRequest request, CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Movie> AddAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<Movie> UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<Movie> DeleteAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<List<string>> GetGenreNamesByMovieIdAsync(long movieId, CancellationToken ct);
    Task<List<Movie>> GetRecommendedAsync(Dictionary<string, decimal> weightMap, List<long> excludeIds, int count, CancellationToken ct);
    Task<List<Movie>> GetPopularAsync(List<long> excludeIds, int count, CancellationToken ct);
}
