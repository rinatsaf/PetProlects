using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserPreferenceRepository(CinemaDbContext context) : IUserPreferenceRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<IReadOnlyList<UserPreference>> GetByUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserPreferences
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(UserPreference preference, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserPreferences
            .FirstOrDefaultAsync(x =>
                x.UserId == preference.UserId &&
                x.FeatureKey == preference.FeatureKey, cancellationToken);

        if (existing is null)
        {
            _context.UserPreferences.Add(preference);
        }
        else
        {
            existing.Weight += preference.Weight;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            _context.UserPreferences.Update(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task UpsertRangeAsync(IEnumerable<UserPreference> preferences, CancellationToken cancellationToken = default)
    {
        var preferenceList = preferences.ToList();
        if (preferenceList.Count == 0)
        {
            return;
        }

        var userId = preferenceList[0].UserId;
        var keys = preferenceList.Select(p => p.FeatureKey).ToList();

        var existingPrefs = await _context.UserPreferences
            .Where(x => x.UserId == userId && keys.Contains(x.FeatureKey))
            .ToListAsync(cancellationToken);

        foreach (var pref in preferenceList)
        {
            var existing = existingPrefs.FirstOrDefault(x => x.FeatureKey == pref.FeatureKey);
            if (existing is null)
            {
                _context.UserPreferences.Add(pref);
            }
            else
            {
                existing.Weight += pref.Weight;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                _context.UserPreferences.Update(existing);
            }
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ApplyDecayAsync(decimal decayFactor, decimal deleteThreshold, CancellationToken cancellationToken = default)
    {
        if (decayFactor <= 0 || decayFactor >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(decayFactor), "Decay factor must be between 0 and 1.");
        }

        if (deleteThreshold < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deleteThreshold), "Delete threshold cannot be negative.");
        }

        var now = DateTimeOffset.UtcNow;

        var updatedCount = await _context.UserPreferences
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Weight, x => x.Weight * decayFactor)
                .SetProperty(x => x.UpdatedAt, now), cancellationToken);

        var deletedCount = await _context.UserPreferences
            .Where(x => Math.Abs(x.Weight) < deleteThreshold)
            .ExecuteDeleteAsync(cancellationToken);

        return updatedCount + deletedCount;
    }
}


