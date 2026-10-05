using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class RecommendationService(
    IUserPreferenceRepository prefRepo,
    IUserInteractionRepository interactionRepo,
    IMovieRepository movieRepo) : IRecommendationService
{
    public async Task<List<Movie>> GetRecommendationsAsync(long userId, int count, CancellationToken ct = default)
    {
        var prefs = await prefRepo.GetByUserAsync(userId, ct);
        var interactions = await interactionRepo.GetByUserAsync(userId, InteractionType.BuyTicket, ct);
        
        var weightMap = prefs.ToDictionary(x => x.FeatureKey, x => x.Weight);
        var excludeIds = interactions.Select(i => i.MovieId).Distinct().ToList();
        
        if (weightMap.Count == 0)
        {
            return await movieRepo.GetPopularAsync(excludeIds, count, ct);
        }
        
        return await movieRepo.GetRecommendedAsync(weightMap, excludeIds, count, ct);
    }

    public async Task TrackInteractionAsync(long userId, long movieId, InteractionType type, CancellationToken ct = default)
    {
        decimal delta = GetDelta(type);
        var interaction = new UserInteraction
        {
            UserId = userId,
            MovieId = movieId,
            ActionType = type,
            WeightDelta = delta,
            User = null,
            Movie = null
        };
        await interactionRepo.AddAsync(interaction, ct);
        
        var genreNames = await movieRepo.GetGenreNamesByMovieIdAsync(movieId, ct);

        var preferences = genreNames.Select(name => new UserPreference
        {
            UserId = userId,
            FeatureKey = name,
            Weight = delta,
            User = null
        });

        await prefRepo.UpsertRangeAsync(preferences, ct);
    }

    private decimal GetDelta(InteractionType type) => type switch
    {
        InteractionType.BuyTicket => 5.0m,
        InteractionType.OpenDetails => 1.0m,
        InteractionType.Click => 0.5m,
        _ => 0.2m
    };
}
