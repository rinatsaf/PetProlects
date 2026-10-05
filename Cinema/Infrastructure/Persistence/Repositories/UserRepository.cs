using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class UserRepository(CinemaDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.Users
                .AnyAsync(u => u.Id == id, cancellationToken);
            
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> UpdateRoleAsync(long id, UserRole role, CancellationToken cancellationToken = default)
    {
        var updated = await context.Users
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Role, role)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), cancellationToken: cancellationToken);
        
        return updated > 0;
    }

    public async Task<bool> UpdateStatusAsync(long id, bool isActive, CancellationToken cancellationToken = default)
    {
        var updated = await context.Users
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsActive, isActive)
                .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken);

        return updated > 0;
    }
}



