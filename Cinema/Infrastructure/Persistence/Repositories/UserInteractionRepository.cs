using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserInteractionRepository(CinemaDbContext context) : IUserInteractionRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task AddAsync(UserInteraction interaction, CancellationToken cancellationToken = default)
    {
        await _context.UserInteractions.AddAsync(interaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserInteraction>> GetByUserAsync(long userId, InteractionType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _context.UserInteractions
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (type.HasValue)
        {
            query = query.Where(x => x.ActionType == type.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
