namespace TouristRoutes.Models.Entity;

public class Poi
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string Category { get; set; }
    public int VisitDurationMin { get; set; }
    public int EntranceFeeCents { get; set; }
    public string? OpeningHoursJson { get; set; }
    public string AudioFilePath { get; set; }
    public double? Rating { get; set; }
    public string? Description { get; set; } = null;
    public double DistanceKm { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CityId { get; set; }

    public City City { get; set; }
    public List<TourRoute> Routes { get; set; } = new();
}