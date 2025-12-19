namespace TouristRoutes.Models.DTOs.Request;

public class SaveRouteRequest
{
    public string? Name { get; set; }
    public int UserId { get; set; }
    public List<int> PoiIds { get; set; } = new();
}