namespace NotificationNoise.Ingestion.Application;

public interface IGoogleOAuthClient
{
    string BuildAuthorizationUrl(string clientId, string redirectUri, string[] scopes, string state);
    Task<OAuthTokenResponse> ExchangeCodeAsync(string code, string clientId, string clientSecret, string redirectUri, CancellationToken ct);
    Task<OAuthTokenResponse> RefreshAsync(string refreshToken, string clientId, string clientSecret, CancellationToken ct);
}
