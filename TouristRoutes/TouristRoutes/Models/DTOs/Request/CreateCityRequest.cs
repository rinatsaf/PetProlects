namespace TouristRoutes.Models.DTOs.Request;

public class CreateCityRequest
{
    public string Name { get; set; }
    public double BboxMinLat { get; set; }
    public double BboxMinLon { get; set; }
    public double BboxMaxLat { get; set; }
    public double BboxMaxLon { get; set; }
}
