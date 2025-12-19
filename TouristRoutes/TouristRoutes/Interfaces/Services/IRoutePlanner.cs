using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Interfaces.Services;

public interface IRoutePlanner
{
    RouteResponse PlanRoute(RouteBuildRequest request, List<Poi> pois);
}