using Application.DTOs.Movies;

namespace Application.Abstractions.Services;

public interface IMovieService
{
    Task<IReadOnlyList<MovieDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MovieDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<MovieDto> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken = default);
    Task<MovieDto> UpdateAsync(long id, UpdateMovieRequest request, CancellationToken cancellationToken = default);
    Task<MovieDto> DeleteAsync(long id, CancellationToken cancellationToken = default);
}