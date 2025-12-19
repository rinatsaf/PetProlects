using TouristRoutes.Models.Entity;

namespace TouristRoutes.Models.DTOs.Response;

public class RoutePoint
{
    public Poi Poi { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public double TravelTimeToNextMin { get; set; }
    public double DistanceToNextKm { get; set; }
}