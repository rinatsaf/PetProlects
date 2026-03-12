namespace NotificationNoise.Ingestion.Application;

public sealed record OAuthSettings(
    string ClientId,
    string ClientSecret,
    string RedirectUri,
    string[] Scopes
);
