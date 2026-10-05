using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IUserPreferenceRepository
{
    Task<IReadOnlyList<UserPreference>> GetByUserAsync(long userId, CancellationToken cancellationToken = default);
    Task UpsertAsync(UserPreference preference, CancellationToken cancellationToken = default);
    Task UpsertRangeAsync(IEnumerable<UserPreference> preferences, CancellationToken cancellationToken = default);
    Task<int> ApplyDecayAsync(decimal decayFactor, decimal deleteThreshold, CancellationToken cancellationToken = default);
}
