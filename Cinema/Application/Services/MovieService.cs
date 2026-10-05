using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Movies;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class MovieService(
    IMovieRepository movieRepository,
    IGenreRepository genreRepository,
    IRecommendationService recommendationService,
    ICurrentUserService currentUserService,
    IMapper mapper) : IMovieService
{
    private readonly IMovieRepository _movieRepository = movieRepository;
    private readonly IGenreRepository _genreRepository = genreRepository;
    private readonly IRecommendationService _recommendationService = recommendationService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<MovieDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var movies = await _movieRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<MovieDto>>(movies);
    }

    public async Task<IReadOnlyList<MovieDto>> SearchAsync(
        MovieSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var safeRequest = request ?? new MovieSearchRequest();
        safeRequest.Limit = Math.Clamp(safeRequest.Limit, 1, 100);
        var movies = await _movieRepository.SearchAsync(safeRequest, cancellationToken);
        return _mapper.Map<IReadOnlyList<MovieDto>>(movies);
    }

    public async Task<MovieDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository
                        .GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException($"Movie with id: {id} not found");

        var currentUser = _currentUserService.GetCurrentUser();
        if (currentUser.IsAuthenticated)
        {
            await _recommendationService.TrackInteractionAsync(
                currentUser.UserId,
                id,
                InteractionType.OpenDetails,
                cancellationToken);
        }

        return _mapper.Map<MovieDto>(movie);
    }

    public async Task<IReadOnlyList<MovieDto>> GetRecommendationsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUserService.GetCurrentUser();
        
        var safeCount = Math.Clamp(count, 1, 50);
        var recommended = await _recommendationService.GetRecommendationsAsync(currentUser.UserId, safeCount, cancellationToken);
        return _mapper.Map<IReadOnlyList<MovieDto>>(recommended);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieRequest request, CancellationToken cancellationToken = default)
    {
        var genreIds = request.GenreIds
            .Distinct()
            .ToArray();

        var genres = await _genreRepository.GetByIdsAsync(genreIds, cancellationToken);
        if (genres.Count != genreIds.Length)
        {
            throw new NotFoundException("One or more genres were not found.");
        }

        var movie = _mapper.Map<Movie>(request);
        Normalize(movie);
        movie.MovieGenres = genreIds
            .Select(genreId => new MovieGenre
            {
                Movie = movie,
                GenreId = genreId,
            })
            .ToList();

        var createdMovie = await _movieRepository.AddAsync(movie, cancellationToken);

        return _mapper.Map<MovieDto>(createdMovie);
    }

    public async Task<MovieDto> UpdateAsync(long id, UpdateMovieRequest request, CancellationToken cancellationToken = default)
    {
        var movie = await _movieRepository.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException($"Movie with id {id} was not found.");

        if (request.GenreIds is not null)
        {
            var genreIds = request.GenreIds
                .Distinct()
                .ToArray();

            var genres = await _genreRepository.GetByIdsAsync(genreIds, cancellationToken);
            if (genres.Count != genreIds.Length)
            {
                throw new NotFoundException("One or more genres were not found.");
            }

            movie.MovieGenres = genreIds
                .Select(genreId => new MovieGenre
                {
                    MovieId = movie.Id,
                    GenreId = genreId,
                })
                .ToList();
        }

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
