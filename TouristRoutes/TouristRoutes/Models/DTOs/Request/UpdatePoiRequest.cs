namespace TouristRoutes.Models.DTOs.Request;

public class UpdatePoiRequest
{
    public int? CityId { get; set; }
    public string? Name { get; set; }
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public string? Category { get; set; }
    public int? VisitDurationMin { get; set; }
    public int? EntranceFeeCents { get; set; }
    public string? OpeningHoursJson { get; set; }
    public string? AudioFilePath { get; set; }
    public int? Rating { get; set; }
    public string? Description { get; set; }
}