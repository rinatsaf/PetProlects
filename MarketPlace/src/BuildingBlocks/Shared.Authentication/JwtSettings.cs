namespace Shared.Authentication;

public sealed record JwtSettings(
    string Secret,
    string Issuer,
    string Audience,
    int ExpiryMinutes = 60);