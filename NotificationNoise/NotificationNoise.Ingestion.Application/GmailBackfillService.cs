using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NotificationNoise.Ingestion.Domain;

namespace NotificationNoise.Ingestion.Application;

public sealed class GmailBackfillService
{
    private static readonly Regex FromRegex = new(
        @"^(?<name>.*?)(?:\s*)<(?<email>[^>]+)>$",
        RegexOptions.Compiled);

    private readonly IGmailMessageClient _gmail;
    private readonly IGmailTokenStore _tokens;
    private readonly IGoogleOAuthClient _oauth;
    private readonly OAuthSettings _settings;
    private readonly INotificationRepository _notifications;
    private readonly INotificationReceivedPublisher _publisher;
    private readonly ILogger<GmailBackfillService> _logger;

    public GmailBackfillService(
        IGmailMessageClient gmail,
        IGmailTokenStore tokens,
        IGoogleOAuthClient oauth,
        OAuthSettings settings,
        INotificationRepository notifications,
        INotificationReceivedPublisher publisher,
        ILogger<GmailBackfillService> logger)
    {
        _gmail = gmail;
        _tokens = tokens;
        _oauth = oauth;
        _settings = settings;
        _notifications = notifications;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<int> BackfillAsync(string userId, int days, int maxMessages, string? label, CancellationToken ct)
    {
        var token = await _tokens.GetAsync(userId, ct);
        if (token is null)
        {
            throw new InvalidOperationException("User has no Gmail tokens. Connect first.");
        }

        token = await EnsureAccessTokenAsync(token, ct);

        var query = BuildQuery(days, label);
        var ids = await _gmail.ListMessageIdsAsync(token.AccessToken, query, maxMessages, ct);
        if (ids.Count == 0)
        {
            return 0;
        }

        var newItems = new List<Notification>();

        foreach (var id in ids)
        {
            var exists = await _notifications.ExistsAsync(userId, "gmail", id, ct);
            if (exists)
            {
                continue;
            }

            var message = await _gmail.GetMessageAsync(token.AccessToken, id, ct);
            if (message is null)
            {
                continue;
            }

            var from = GetHeader(message.Headers, "From");
            var subject = GetHeader(message.Headers, "Subject");
            var listUnsubscribe = GetHeader(message.Headers, "List-Unsubscribe");
            var (fromEmail, fromName) = ParseFrom(from);

            var notification = new Notification(
                userId: userId,
                provider: "gmail",
                externalMessageId: id,
                fromEmail: fromEmail,
                fromName: fromName,
                subject: subject,
                snippet: message.Snippet,
                receivedAt: ParseInternalDate(message.InternalDate),
                hasListUnsubscribe: !string.IsNullOrWhiteSpace(listUnsubscribe));

            newItems.Add(notification);
            _notifications.Add(notification);
        }

        if (newItems.Count == 0)
        {
            return 0;
        }

        await _notifications.SaveChangesAsync(ct);

        foreach (var notification in newItems)
        {
            await _publisher.PublishAsync(notification, ct);
        }

        _logger.LogInformation("Backfill complete: {Count} new messages", newItems.Count);
        return newItems.Count;
    }

    private async Task<GmailTokenData> EnsureAccessTokenAsync(GmailTokenData token, CancellationToken ct)
    {
        if (token.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return token;
        }

        if (string.IsNullOrWhiteSpace(token.RefreshToken))
        {
            throw new InvalidOperationException("Access token expired and no refresh token available.");
        }

        var refreshed = await _oauth.RefreshAsync(token.RefreshToken, _settings.ClientId, _settings.ClientSecret, ct);
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(refreshed.ExpiresIn);

        var updated = new GmailTokenData(
            UserId: token.UserId,
            AccessToken: refreshed.AccessToken,
            RefreshToken: token.RefreshToken,
            ExpiresAt: expiresAt,
            Scope: refreshed.Scope ?? token.Scope,
            TokenType: refreshed.TokenType);

        await _tokens.UpsertAsync(updated, ct);
        return updated;
    }

    private static string BuildQuery(int days, string? label)
    {
        var query = $"newer_than:{days}d";
        if (!string.IsNullOrWhiteSpace(label))
        {
            query += $" label:{label}";
        }

        return query;
    }

    private static string? GetHeader(IReadOnlyList<GmailHeaderData> headers, string name)
    {
        return headers.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    private static (string Email, string? Name) ParseFrom(string? from)
    {
        if (string.IsNullOrWhiteSpace(from))
        {
            return ("unknown@unknown", null);
        }

        var match = FromRegex.Match(from);
        if (!match.Success)
        {
            return (from.Trim(), null);
        }

        var name = match.Groups["name"].Value.Trim().Trim('"');
        var email = match.Groups["email"].Value.Trim();
        return (email, string.IsNullOrWhiteSpace(name) ? null : name);
    }

    private static DateTimeOffset ParseInternalDate(string? internalDate)
    {
        if (long.TryParse(internalDate, out var milliseconds))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        }

        return DateTimeOffset.UtcNow;
    }
}
