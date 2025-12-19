namespace TouristRoutes.Models.DTOs.Response;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Route> RecentRoutes { get; set; }
}