namespace NotificationNoise.Ingestion.Domain;

public sealed class GmailToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string UserId { get; private set; } = default!;
    public string Provider { get; private set; } = "gmail";
    public string AccessTokenEncrypted { get; private set; } = default!;
    public string? RefreshTokenEncrypted { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public string Scope { get; private set; } = default!;
    public string TokenType { get; private set; } = default!;
    public DateTimeOffset UpdatedAt { get; private set; }

    private GmailToken() { }

    public GmailToken(
        string userId,
        string provider,
        string accessTokenEncrypted,
        string? refreshTokenEncrypted,
        DateTimeOffset expiresAt,
        string scope,
        string tokenType,
        DateTimeOffset updatedAt)
    {
        UserId = userId;
        Provider = provider;
        AccessTokenEncrypted = accessTokenEncrypted;
        RefreshTokenEncrypted = refreshTokenEncrypted;
        ExpiresAt = expiresAt;
        Scope = scope;
        TokenType = tokenType;
        UpdatedAt = updatedAt;
    }

    public void Update(
        string accessTokenEncrypted,
        string? refreshTokenEncrypted,
        DateTimeOffset expiresAt,
        string scope,
        string tokenType,
        DateTimeOffset updatedAt)
    {
        AccessTokenEncrypted = accessTokenEncrypted;
        RefreshTokenEncrypted = refreshTokenEncrypted;
        ExpiresAt = expiresAt;
        Scope = scope;
        TokenType = tokenType;
        UpdatedAt = updatedAt;
    }
}
