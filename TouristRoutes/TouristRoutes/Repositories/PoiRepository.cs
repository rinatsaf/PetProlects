using Microsoft.EntityFrameworkCore;
using TouristRoutes.Data;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Repositories;

public class PoiRepository(RoutesDbContext context) : IPoiRepository
{
    private readonly RoutesDbContext _context = context;

    public async Task<Poi> GetPoiByIdAsync(int id)
    {
        var poi = await _context.Pois
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (poi is null)
            throw new KeyNotFoundException("Poi not found");
        
        return poi;
    }

    public async Task<Poi> GetPoiByNameAsync(string name)
    {
        var poi = await _context.Pois
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name == name);
        
        if (poi is null)
            throw new KeyNotFoundException("Poi not found");

        return poi;
    }

    public async Task<List<Poi>> GetPoisByCityId(int id)
    {
        var pois = await _context.Pois
            .AsNoTracking()
            .Where(p => p.CityId == id)
            .ToListAsync();
        
        return pois;
    }

    public async Task<Poi> CreatePoiAsync(Poi poi)
    {
        await _context.Pois.AddAsync(poi);
        await _context.SaveChangesAsync();
        return poi;
    }

    public async Task<Poi> UpdatePoiAsync(Poi poi)
    {
        var existingPoi = await _context.Pois.FindAsync(poi.Id);
        if (existingPoi is null)
            throw new KeyNotFoundException("Poi not found");
        
        _context.Pois.Update(poi);
        await _context.SaveChangesAsync();
        return poi;
    }

    public async Task DeletePoiByIdAsync(int id)
    {
        var poi = await _context.Pois.FindAsync(id);
        
        if (poi is null)
            throw new KeyNotFoundException("Poi not found");
        
        _context.Pois.Remove(poi);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePoiByNameAsync(string name)
    {
        var poi = await _context.Pois
            .FirstOrDefaultAsync(p => p.Name == name);
        
        if (poi is null)
            throw new KeyNotFoundException("Poi not found");
        
        _context.Pois.Remove(poi);
        await _context.SaveChangesAsync();
    }
}