using Domain.Enums;

namespace Application.DTOs.Auth;

public sealed class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; } 
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
