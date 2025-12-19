using TouristRoutes.Models.DTOs.Response;

namespace TouristRoutes.Models.Entity;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string HashPassword { get; set; }
    public DateTime CreatedAt { get; set; }
    public Role Role { get; set; } = Role.User;
    public List<TourRoute> Route { get; set; } = new();
}