using TouristRoutes.Models.Entity;

namespace TouristRoutes.Interfaces.Repositories;

public interface IRouteRepository
{
    Task<List<TourRoute>> GetAllRoutesAsync();
    Task<List<TourRoute>> GetAllRoutesByUserAsync(int userId);
    Task<List<TourRoute>> GetAllRoutesByUserAsync(string email);
    Task<TourRoute> GetTourRouteByIdAsync(int routeId);

    Task<TourRoute> SaveRouteAsync(TourRoute route);
    Task<TourRoute> UpdateRouteAsync(TourRoute route);
    
    Task DeleteRouteByIdAsync(int id);
}