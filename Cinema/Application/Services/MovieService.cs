using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Movies;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class MovieService(IMovieRepository movieRepository, IMapper mapper) : IMovieService
{
    private readonly IMovieRepository _movieRepository = movieRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<MovieDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var movies = await _movieRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<MovieDto>>(movies);
    }

    public async Task<MovieDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository
                        .GetByIdAsync(id, cancellationToken) 
                    ?? throw new NotFoundException($"Movie with id: {id} not found");
        
        return _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken = default)
    {
        var movie = _mapper.Map<Movie>(request);
        Normalize(movie);

        var createdMovie = await _movieRepository.AddAsync(movie, cancellationToken);

        return _mapper.Map<MovieDto>(createdMovie);
    }

    public async Task<MovieDto> UpdateAsync(long id, UpdateMovieRequest request, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException($"Movie with id {id} was not found.");
        

        _mapper.Map(request, movie);
        Normalize(movie);

        movie.UpdatedAt = DateTimeOffset.UtcNow;
        
        var updatedMovie = await _movieRepository.UpdateAsync(movie, cancellationToken);
        return _mapper.Map<MovieDto>(updatedMovie);
    }

    public async Task<MovieDto> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken);

        if (movie is null)
        {
            throw new NotFoundException($"Movie with id {id} was not found.");
        }

        var deletedMovie = await _movieRepository.DeleteAsync(movie, cancellationToken);

        return _mapper.Map<MovieDto>(deletedMovie);
    }

    private static void Normalize(Movie movie)
    {
        movie.Title = movie.Title.Trim();
        movie.Description = movie.Description.Trim();
        movie.AgeRating = movie.AgeRating.Trim();
        movie.Country = movie.Country.Trim();
        movie.PosterUrl = string.IsNullOrWhiteSpace(movie.PosterUrl) ? null : movie.PosterUrl.Trim();
    }
}
