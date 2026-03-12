namespace NotificationNoise.Ingestion.Application;

public sealed class GmailOAuthService
{
    private readonly IGoogleOAuthClient _oauth;
    private readonly IGmailTokenStore _store;
    private readonly OAuthSettings _settings;

    public GmailOAuthService(IGoogleOAuthClient oauth, IGmailTokenStore store, OAuthSettings settings)
    {
        _oauth = oauth;
        _store = store;
        _settings = settings;
    }

    public string BuildAuthUrl(string userId)
    {
        return _oauth.BuildAuthorizationUrl(
            clientId: _settings.ClientId,
            redirectUri: _settings.RedirectUri,
            scopes: _settings.Scopes,
            state: userId);
    }

    public async Task ExchangeCodeAsync(string userId, string code, CancellationToken ct)
    {
        var token = await _oauth.ExchangeCodeAsync(
            code: code,
            clientId: _settings.ClientId,
            clientSecret: _settings.ClientSecret,
            redirectUri: _settings.RedirectUri,
            ct: ct);

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
        var data = new GmailTokenData(
            UserId: userId,
            AccessToken: token.AccessToken,
            RefreshToken: token.RefreshToken,
            ExpiresAt: expiresAt,
            Scope: token.Scope ?? string.Join(' ', _settings.Scopes),
            TokenType: token.TokenType);

        await _store.UpsertAsync(data, ct);
    }
}
