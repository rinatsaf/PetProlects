using TouristRoutes.Models.Entity;

namespace TouristRoutes.Interfaces.Repositories;

public interface IPoiRepository
{
    Task<Poi> GetPoiByIdAsync(int id);
    Task<Poi> GetPoiByNameAsync(string name);
    Task<List<Poi>> GetPoisByCityId(int id);
    
    Task<Poi> CreatePoiAsync(Poi poi);
    Task<Poi> UpdatePoiAsync(Poi poi);
    
    Task DeletePoiByIdAsync(int id);
    Task DeletePoiByNameAsync(string name);
}