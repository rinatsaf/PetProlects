namespace TouristRoutes.Models.Entity;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double? BboxMinLat { get; set; }
    public double? BboxMinLon { get; set; }
    public double? BboxMaxLat { get; set; }
    public double? BboxMaxLon { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<Poi> Pois { get; set; } = new();
}