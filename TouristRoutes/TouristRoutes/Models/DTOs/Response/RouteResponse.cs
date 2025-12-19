namespace TouristRoutes.Models.DTOs.Response;

public class RouteResponse
{
    public int Id { get; set; }
    public required string  Name { get; set; }
    public int UserId { get; set; }
    public List<RoutePoint> Points { get; set; } = new();
    public decimal TotalCost { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan TotalTravelTime { get; set; }
    public double TotalDistanceKm { get; set; }
    public string RouteSummary { get; set; }
}
