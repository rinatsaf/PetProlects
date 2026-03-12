namespace NotificationNoise.Ingestion.Application;

public sealed record OAuthTokenResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresIn,
    string? Scope,
    string TokenType
);
