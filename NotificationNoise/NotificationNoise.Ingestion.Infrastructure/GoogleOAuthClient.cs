using System.Net.Http.Json;
using System.Text.Json.Serialization;
using NotificationNoise.Ingestion.Application;

namespace NotificationNoise.Ingestion.Infrastructure;

public sealed class GoogleOAuthClient : IGoogleOAuthClient
{
    private readonly HttpClient _http;

    public GoogleOAuthClient(HttpClient http)
    {
        _http = http;
    }

    public string BuildAuthorizationUrl(string clientId, string redirectUri, string[] scopes, string state)
    {
        var scope = string.Join(' ', scopes);
        var url =
            "https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            "&response_type=code" +
            $"&scope={Uri.EscapeDataString(scope)}" +
            "&access_type=offline" +
            "&include_granted_scopes=true" +
            "&prompt=consent" +
            $"&state={Uri.EscapeDataString(state)}";

        return url;
    }

    public async Task<OAuthTokenResponse> ExchangeCodeAsync(
        string code,
        string clientId,
        string clientSecret,
        string redirectUri,
        CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        };

        using var res = await _http.PostAsync("/token", new FormUrlEncodedContent(form), ct);
        res.EnsureSuccessStatusCode();

        var dto = await res.Content.ReadFromJsonAsync<TokenResponseDto>(cancellationToken: ct);
        if (dto is null) throw new InvalidOperationException("OAuth response is empty");

        return new OAuthTokenResponse(
            AccessToken: dto.AccessToken,
            RefreshToken: dto.RefreshToken,
            ExpiresIn: dto.ExpiresIn,
            Scope: dto.Scope,
            TokenType: dto.TokenType);
    }

    public async Task<OAuthTokenResponse> RefreshAsync(
        string refreshToken,
        string clientId,
        string clientSecret,
        CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["grant_type"] = "refresh_token"
        };

        using var res = await _http.PostAsync("/token", new FormUrlEncodedContent(form), ct);
        res.EnsureSuccessStatusCode();

        var dto = await res.Content.ReadFromJsonAsync<TokenResponseDto>(cancellationToken: ct);
        if (dto is null) throw new InvalidOperationException("OAuth response is empty");

        return new OAuthTokenResponse(
            AccessToken: dto.AccessToken,
            RefreshToken: refreshToken,
            ExpiresIn: dto.ExpiresIn,
            Scope: dto.Scope,
            TokenType: dto.TokenType);
    }

    private sealed class TokenResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = default!;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("scope")]
        public string? Scope { get; init; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = default!;
    }
}
