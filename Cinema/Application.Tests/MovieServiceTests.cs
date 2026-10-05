using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Movies;
using Application.Exceptions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests;

public class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _movieRepo = new();
    private readonly Mock<IGenreRepository> _genreRepo = new();
    private readonly Mock<IRecommendationService> _recommendation = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly MovieService _sut;

    public MovieServiceTests()
    {
        _sut = new MovieService(_movieRepo.Object, _genreRepo.Object, _recommendation.Object, _currentUser.Object, _mapper.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMovies()
    {
        var movies = new List<Movie> { new() { Id = 1, Title = "A", Description = "D", AgeRating = "12+", Country = "RU" } };
        _movieRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(movies);
        _mapper.Setup(x => x.Map<IReadOnlyList<MovieDto>>(movies))
            .Returns(new List<MovieDto> { new() { Id = 1 } });

        var result = await _sut.GetAllAsync();
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_ClampsLimitToRange()
    {
        _movieRepo.Setup(x => x.SearchAsync(It.IsAny<MovieSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>());
        _mapper.Setup(x => x.Map<IReadOnlyList<MovieDto>>(It.IsAny<List<Movie>>()))
            .Returns(new List<MovieDto>());

        await _sut.SearchAsync(new MovieSearchRequest { Limit = 999 });
        _movieRepo.Verify(x => x.SearchAsync(
            It.Is<MovieSearchRequest>(r => r.Limit == 100), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task SearchAsync_WithNullRequest_ReturnsEmptyResults()
    {
        _movieRepo.Setup(x => x.SearchAsync(It.IsAny<MovieSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>());
        _mapper.Setup(x => x.Map<IReadOnlyList<MovieDto>>(It.IsAny<List<Movie>>()))
            .Returns(new List<MovieDto>());

        var result = await _sut.SearchAsync(null);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_TracksViewForAuthUser()
    {
        var movie = new Movie { Id = 1, Title = "A", Description = "D", AgeRating = "12+", Country = "RU" };
        _movieRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(new CurrentUserInfo { IsAuthenticated = true, UserId = 10 });
        _mapper.Setup(x => x.Map<MovieDto>(movie)).Returns(new MovieDto { Id = 1 });

        await _sut.GetByIdAsync(1);

        _recommendation.Verify(x => x.TrackInteractionAsync(10, 1, InteractionType.OpenDetails, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _movieRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task GetByIdAsync_DoesNotTrackForAnonymous()
    {
        var movie = new Movie { Id = 1, Title = "A", Description = "D", AgeRating = "12+", Country = "RU" };
        _movieRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(new CurrentUserInfo { IsAuthenticated = false });
        _mapper.Setup(x => x.Map<MovieDto>(movie)).Returns(new MovieDto { Id = 1 });

        await _sut.GetByIdAsync(1);

        _recommendation.Verify(x => x.TrackInteractionAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<InteractionType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithAllValidGenres_CreatesMovie()
    {
        var request = new CreateMovieRequest
        {
            Title = "New", Description = "D", DurationMinutes = 120, AgeRating = "16+", Country = "RU",
            GenreIds = new long[] { 1, 2 }
        };
        _genreRepo.Setup(x => x.GetByIdsAsync(new long[] { 1, 2 }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Genre> { new() { Id = 1 }, new() { Id = 2 } });
        _mapper.Setup(x => x.Map<Movie>(request)).Returns(new Movie { Title = "New", Description = "D", AgeRating = "16+", Country = "RU" });
        _movieRepo.Setup(x => x.AddAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Movie { Id = 5, Title = "New", Description = "D", AgeRating = "16+", Country = "RU" });
        _mapper.Setup(x => x.Map<MovieDto>(It.IsAny<Movie>())).Returns(new MovieDto { Id = 5 });

        var result = await _sut.CreateAsync(request);
        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WithMissingGenre_ThrowsNotFound()
    {
        var request = new CreateMovieRequest
        {
            Title = "New", Description = "D", DurationMinutes = 120, AgeRating = "16+", Country = "RU",
            GenreIds = new long[] { 1, 99 }
        };
        _genreRepo.Setup(x => x.GetByIdsAsync(It.IsAny<long[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Genre> { new() { Id = 1 } });

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_WithGenreIdsNull_DoesNotFetchGenres()
    {
        var movie = new Movie { Id = 1, Title = "Old", Description = "D", AgeRating = "16+", Country = "RU" };
        _movieRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _mapper.Setup(x => x.Map<MovieDto>(It.IsAny<Movie>())).Returns(new MovieDto { Id = 1 });

        await _sut.UpdateAsync(1, new UpdateMovieRequest { Title = "Updated", Description = "D", DurationMinutes = 120, AgeRating = "16+", Country = "RU", ReleaseDate = DateOnly.FromDateTime(DateTime.Today), PopularityScore = 5, IsActive = true });

        _genreRepo.Verify(x => x.GetByIdsAsync(It.IsAny<long[]>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_DeletesAndReturns()
    {
        var movie = new Movie { Id = 1, Title = "A", Description = "D", AgeRating = "12+", Country = "RU" };
        _movieRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _movieRepo.Setup(x => x.DeleteAsync(movie, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _mapper.Setup(x => x.Map<MovieDto>(movie)).Returns(new MovieDto { Id = 1 });

        var result = await _sut.DeleteAsync(1);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ThrowsNotFound()
    {
        _movieRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(99));
    }

    [Fact]
    public async Task GetRecommendationsAsync_ClampsCount()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(new CurrentUserInfo { UserId = 1 });
        _recommendation.Setup(x => x.GetRecommendationsAsync(1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>());
        _mapper.Setup(x => x.Map<IReadOnlyList<MovieDto>>(It.IsAny<List<Movie>>()))
            .Returns(new List<MovieDto>());

        await _sut.GetRecommendationsAsync(999);
        _recommendation.Verify(x => x.GetRecommendationsAsync(1, 50, It.IsAny<CancellationToken>()));
    }
}
