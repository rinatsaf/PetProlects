namespace IdentityService.Application;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiry,
    string Role
);