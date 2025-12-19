using Microsoft.EntityFrameworkCore;
using TouristRoutes.Data;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Repositories;

public class RouteRepository(RoutesDbContext context) : IRouteRepository
{
    private readonly RoutesDbContext _context = context;
    
    public async Task<List<TourRoute>> GetAllRoutesAsync()
    {
        return await _context.Routes
            .Include(r => r.Pois)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<TourRoute>> GetAllRoutesByUserAsync(int userId)
    {
        return await _context.Routes
            .Where(r => r.UserId == userId)
            .Include(r => r.Pois)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<TourRoute>> GetAllRoutesByUserAsync(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            throw new KeyNotFoundException($"User with email {email} not found");
        
        return await _context.Routes
            .Where(r => r.UserId == user.Id)
            .Include(r => r.Pois)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<TourRoute> GetTourRouteByIdAsync(int routeId)
    {
        var  route = await _context.Routes
            .Include(r => r.Pois)
            .FirstOrDefaultAsync(r => r.Id == routeId);
        if (route is null)
            throw new KeyNotFoundException($"Route with id {routeId} not found");
        
        return route;
    }

    public async Task<TourRoute> SaveRouteAsync(TourRoute route)
    {
        await _context.Routes.AddAsync(route);
        await _context.SaveChangesAsync();
        
        return route;
    }

    public async Task<TourRoute> UpdateRouteAsync(TourRoute route)
    {
        _context.Routes.Update(route);
        await _context.SaveChangesAsync();
        return route;
    }

    public async Task DeleteRouteByIdAsync(int id)
    {
        var route = await _context.Routes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (route is null)
            throw new KeyNotFoundException();
        
        _context.Routes.Remove(route);
        await _context.SaveChangesAsync();
    }
}
