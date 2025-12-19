using Microsoft.EntityFrameworkCore;
using TouristRoutes.Data;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Repositories;

public class UserRepository(RoutesDbContext context) : IUserRepository
{
    private readonly RoutesDbContext _context = context;
    
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await context.Users
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            throw new KeyNotFoundException("User not found");

        return user;
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            throw new KeyNotFoundException("User not found");
        
        return user;
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
        
        if (user is null)
            throw new KeyNotFoundException("User not found");
        
        return user;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }


    // Полное обновление
    public async Task<User> UpdateUserAsync(User user)
    {
        var existing = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == user.Id);

        if (existing is null)
            throw new KeyNotFoundException("User not found");
        
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user is null)
            throw new KeyNotFoundException("User not found");
        
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }

    public async Task DeleteUserByEmailAsync(string email)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user is null)
            throw new KeyNotFoundException("User not found");
        
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}