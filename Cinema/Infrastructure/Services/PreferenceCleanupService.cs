using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class PreferenceCleanupService(
    IUserPreferenceRepository preferenceRepository,
    IOptionsSnapshot<PreferenceCleanupOptions> options) : IPreferenceCleanupService
{
    private readonly IUserPreferenceRepository _preferenceRepository = preferenceRepository;
    private readonly decimal _decayFactor = options.Value.DecayFactor;
    private readonly decimal _deleteThreshold = options.Value.DeleteThreshold;

    public async Task<int> ApplyWeeklyDecayAsync(CancellationToken cancellationToken = default)
    {
        return await _preferenceRepository.ApplyDecayAsync(_decayFactor, _deleteThreshold, cancellationToken);
    }
}
