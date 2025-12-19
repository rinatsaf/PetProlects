using System.ComponentModel.DataAnnotations;

namespace TouristRoutes.Models.DTOs.Request;

public class CreateUserRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}