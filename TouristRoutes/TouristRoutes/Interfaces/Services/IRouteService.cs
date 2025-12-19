using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;

namespace TouristRoutes.Interfaces.Services;

public interface IRouteService
{
    Task<List<RouteResponse>> GetAllRoutesAsync();
    Task<List<RouteResponse>> GetAllRoutesByUserAsync(int userId);
    Task<List<RouteResponse>> GetAllRoutesByUserEmailAsync(string email);

    Task<RouteResponse> CreateRouteAsync(RouteBuildRequest request, int userId);
    Task<RouteResponse> PreviewRouteAsync(RouteBuildRequest request, int userId);

    Task<RouteResponse> UpdateRouteAsync(int id, UpdateRouteRequest request);

    Task DeleteRouteByIdAsync(int id);
}
