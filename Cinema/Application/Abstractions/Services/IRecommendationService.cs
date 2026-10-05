using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IRecommendationService
{
    Task<List<Movie>> GetRecommendationsAsync(long userId, int count, CancellationToken ct = default);
    Task TrackInteractionAsync(long userId, long movieId, InteractionType type, CancellationToken ct = default);
}
