using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Genres;
using Application.Exceptions;
using AutoMapper;

namespace Application.Services;

public class GenreService(IGenreRepository genreRepository, IMapper mapper) : IGenreService
{
    private readonly IGenreRepository _genreRepository = genreRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var genres = await _genreRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<GenreDto>>(genres);
    }

    public async Task<GenreDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var genre = await _genreRepository.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException($"Genre with id {id} not found");
        return _mapper.Map<GenreDto>(genre);
    }

    public async Task<GenreDto> CreateAsync(CreateGenreRequest request, CancellationToken cancellationToken = default)
    {
        if (await _genreRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new ConflictException("Genre with the same name already exists");
        }

        var genre = _mapper.Map<Domain.Entities.Genre>(request);
        var created = await _genreRepository.AddAsync(genre, cancellationToken);
        return _mapper.Map<GenreDto>(created);
    }

    public async Task<GenreDto> UpdateAsync(long id, UpdateGenreRequest request, CancellationToken cancellationToken = default)
    {
        var genre = await _genreRepository.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException($"Genre with id {id} not found");

        if (await _genreRepository.ExistsByNameAsync(request.Name, id, cancellationToken))
        {
            throw new ConflictException("Genre with the same name already exists");
        }

        _mapper.Map(request, genre);
        genre.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _genreRepository.UpdateAsync(genre, cancellationToken);
        return _mapper.Map<GenreDto>(updated);
    }

    public async Task<GenreDto> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var genre = await _genreRepository.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException($"Genre with id {id} not found");

        var deleted = await _genreRepository.DeleteAsync(genre, cancellationToken);
        return _mapper.Map<GenreDto>(deleted);
    }
}
