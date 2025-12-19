using TouristRoutes.Models.Entity;

namespace TouristRoutes.Interfaces.Repositories;

public interface ICityRepository
{
    Task<List<City>> GetAllCitiesAsync();
    Task<City> GetCityByNameAsync(string name);
    Task<City> GetCityByIdAsync(int id);
    
    Task<City> CreateCityAsync(City city);
    
    Task<City> UpdateCityAsync(City city);
    
    Task DeleteCityByIdAsync(int id);
    Task DeleteCityByNameAsync(string name);
}