namespace TouristRoutes.Models.Entity;

public class TourRoute
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public decimal TotalCost { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan TotalTravelTime { get; set; }
    public double TotalDistanceKm { get; set; }
    public string RouteSummary { get; set; }
    public List<Poi> Pois { get; set; } = new();
    public User User { get; set; }
}