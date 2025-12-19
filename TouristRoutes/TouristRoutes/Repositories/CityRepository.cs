using Microsoft.EntityFrameworkCore;
using TouristRoutes.Data;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Repositories;

public class CityRepository(RoutesDbContext context) : ICityRepository
{
    private readonly RoutesDbContext _context = context;
    
    public async Task<List<City>> GetAllCitiesAsync()
    {
        return await context.Cities
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<City> GetCityByNameAsync(string name)
    {
        var city = await context.Cities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name);

        if (city is null)
            throw new KeyNotFoundException();
        
        return city;
    }

    public async Task<City> GetCityByIdAsync(int id)
    {
        var city = await context.Cities
            .FindAsync(id);
        
        if (city is null)
            throw new KeyNotFoundException();
        
        return city;
    }

    public async Task<City> CreateCityAsync(City city)
    {
        await context.Cities.AddAsync(city);
        await context.SaveChangesAsync();
        return city;
    }

    public async Task<City> UpdateCityAsync(City city)
    {
        var existingCity = await context.Cities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == city.Id);
        
        if (existingCity is null)
            throw new KeyNotFoundException();
        
        context.Cities.Update(city);
        await context.SaveChangesAsync();
        return city;
    }

    public async Task DeleteCityByIdAsync(int id)
    {
        var city = await context.Cities
            .FindAsync(id);
        
        if (city is null)
            throw new KeyNotFoundException();
        
        context.Cities.Remove(city);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCityByNameAsync(string name)
    {
        var city = await context.Cities
            .FirstOrDefaultAsync(c => c.Name == name);
        
        if (city is null)
            throw new KeyNotFoundException();
        
        context.Cities.Remove(city);
        await context.SaveChangesAsync();
    }
}