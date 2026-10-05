using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests;

public class RecommendationServiceTests
{
    private readonly Mock<IUserPreferenceRepository> _prefRepo = new();
    private readonly Mock<IUserInteractionRepository> _interactionRepo = new();
    private readonly Mock<IMovieRepository> _movieRepo = new();
    private readonly RecommendationService _sut;

    public RecommendationServiceTests()
    {
        _sut = new RecommendationService(_prefRepo.Object, _interactionRepo.Object, _movieRepo.Object);
    }

    [Fact]
    public async Task GetRecommendationsAsync_WithPreferences_ReturnsRecommended()
    {
        var prefs = new List<UserPreference>
        {
            new() { UserId = 1, FeatureKey = "Action", Weight = 5.0m, User = null! }
        };
        _prefRepo.Setup(x => x.GetByUserAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(prefs);
        _interactionRepo.Setup(x => x.GetByUserAsync(1, InteractionType.BuyTicket, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserInteraction>());
        _movieRepo.Setup(x => x.GetRecommendedAsync(
                It.IsAny<Dictionary<string, decimal>>(), It.IsAny<List<long>>(), 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>
            {
                new() { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" }
            });

        var result = await _sut.GetRecommendationsAsync(1, 10);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetRecommendationsAsync_WithoutPreferences_ReturnsPopular()
    {
        _prefRepo.Setup(x => x.GetByUserAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserPreference>());
        _interactionRepo.Setup(x => x.GetByUserAsync(1, InteractionType.BuyTicket, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserInteraction>());
        _movieRepo.Setup(x => x.GetPopularAsync(It.IsAny<List<long>>(), 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>
            {
                new() { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" }
            });

        var result = await _sut.GetRecommendationsAsync(1, 10);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ExcludesPurchasedMovies()
    {
        var prefs = new List<UserPreference>
        {
            new() { UserId = 1, FeatureKey = "Action", Weight = 5.0m, User = null! }
        };
        _prefRepo.Setup(x => x.GetByUserAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(prefs);
        _interactionRepo.Setup(x => x.GetByUserAsync(1, InteractionType.BuyTicket, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserInteraction>
            {
                new() { MovieId = 5, User = null!, Movie = null! }
            });

        await _sut.GetRecommendationsAsync(1, 10);

        await _sut.GetRecommendationsAsync(1, 10);

        _movieRepo.Verify(x => x.GetRecommendedAsync(
            It.IsAny<Dictionary<string, decimal>>(),
            It.Is<List<long>>(ids => ids.Contains(5)),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TrackInteractionAsync_CreatesInteractionAndUpsertsPreferences()
    {
        _movieRepo.Setup(x => x.GetGenreNamesByMovieIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Action", "Comedy" });
        _interactionRepo.Setup(x => x.AddAsync(It.IsAny<UserInteraction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _prefRepo.Setup(x => x.UpsertRangeAsync(It.IsAny<IEnumerable<UserPreference>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.TrackInteractionAsync(1, 1, InteractionType.BuyTicket);

        _interactionRepo.Verify(x => x.AddAsync(It.Is<UserInteraction>(i =>
            i.UserId == 1 && i.MovieId == 1 && i.WeightDelta == 5.0m), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TrackInteractionAsync_UpsertsPreferenceForEachGenre()
    {
        _movieRepo.Setup(x => x.GetGenreNamesByMovieIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Action", "Comedy" });

        await _sut.TrackInteractionAsync(1, 1, InteractionType.OpenDetails);

        _prefRepo.Verify(x => x.UpsertRangeAsync(
            It.Is<IEnumerable<UserPreference>>(prefs =>
                prefs.Count() == 2 &&
                prefs.Any(p => p.FeatureKey == "Action" && p.Weight == 1.0m) &&
                prefs.Any(p => p.FeatureKey == "Comedy" && p.Weight == 1.0m)),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TrackInteractionAsync_OpenDetails_AddsDeltaOfOne()
    {
        _movieRepo.Setup(x => x.GetGenreNamesByMovieIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        await _sut.TrackInteractionAsync(1, 1, InteractionType.OpenDetails);

        _interactionRepo.Verify(x => x.AddAsync(It.Is<UserInteraction>(i =>
            i.WeightDelta == 1.0m), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TrackInteractionAsync_Click_AddsDeltaOfHalf()
    {
        _movieRepo.Setup(x => x.GetGenreNamesByMovieIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        await _sut.TrackInteractionAsync(1, 1, InteractionType.Click);

        _interactionRepo.Verify(x => x.AddAsync(It.Is<UserInteraction>(i =>
            i.WeightDelta == 0.5m), It.IsAny<CancellationToken>()));
    }
}
