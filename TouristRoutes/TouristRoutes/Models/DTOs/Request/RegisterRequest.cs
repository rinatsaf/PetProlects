using TouristRoutes.Models.Entity;

namespace TouristRoutes.Models.DTOs.Request;

public class RegisterRequest
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
}