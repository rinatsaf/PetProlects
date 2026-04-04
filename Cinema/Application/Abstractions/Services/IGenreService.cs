using Application.DTOs.Genres;

namespace Application.Abstractions.Services;

public interface IGenreService
{
    Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GenreDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<GenreDto> CreateAsync(CreateGenreRequest request, CancellationToken cancellationToken = default);
    Task<GenreDto> UpdateAsync(long id, UpdateGenreRequest request, CancellationToken cancellationToken = default);
    Task<GenreDto> DeleteAsync(long id, CancellationToken cancellationToken = default);
}