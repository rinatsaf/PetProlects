namespace TouristRoutes.Models.DTOs.Request;

public class RouteBuildRequest
{
    public required string Name { get; set; }
    public required string City { get; set; }
    public decimal Budget { get; set; }
    public double UserLat { get; set; }
    public double UserLon { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string TransportType { get; set; } = "Walk";
    public List<string> PreferredCategories { get; set; } = new();
}