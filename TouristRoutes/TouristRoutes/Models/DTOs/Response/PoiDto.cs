namespace TouristRoutes.Models.DTOs.Response;

public class PoiDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public int VisitDurationMin { get; set; }
    public int EntranceFeeCents { get; set; }
    public string? OpeningHoursJson { get; set; }
    public double? Rating { get; set; }
    public string? Description { get; set; } = null;
    public int CityId { get; set; }
}