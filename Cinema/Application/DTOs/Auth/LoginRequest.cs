namespace Application.DTOs.Auth;

public sealed class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
