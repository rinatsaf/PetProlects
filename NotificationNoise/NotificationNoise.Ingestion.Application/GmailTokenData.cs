namespace NotificationNoise.Ingestion.Application;

public sealed record GmailTokenData(
    string UserId,
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset ExpiresAt,
    string Scope,
    string TokenType
);
