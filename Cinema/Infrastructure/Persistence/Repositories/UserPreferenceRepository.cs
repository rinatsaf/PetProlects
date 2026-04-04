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
            await _context.UserPreferences.AddAsync(preference, cancellationToken);
        }
        else
        {
            existing.Weight = preference.Weight;
            _context.UserPreferences.Update(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
