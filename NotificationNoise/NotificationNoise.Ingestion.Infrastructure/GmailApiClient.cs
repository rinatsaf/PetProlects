using System.Net.Http.Headers;
using System.Net.Http.Json;
using NotificationNoise.Ingestion.Application;

namespace NotificationNoise.Ingestion.Infrastructure;

public sealed class GmailApiClient : IGmailMessageClient
{
    private readonly HttpClient _http;

    public GmailApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<string>> ListMessageIdsAsync(
        string accessToken,
        string query,
        int maxMessages,
        CancellationToken ct)
    {
        var result = new List<string>();
        string? pageToken = null;

        while (result.Count < maxMessages)
        {
            var take = Math.Min(500, maxMessages - result.Count);
            var url = $"messages?q={Uri.EscapeDataString(query)}&maxResults={take}";
            if (!string.IsNullOrEmpty(pageToken))
                url += $"&pageToken={Uri.EscapeDataString(pageToken)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var res = await _http.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();

            var list = await res.Content.ReadFromJsonAsync<GmailListResponse>(cancellationToken: ct);
            if (list?.Messages is null || list.Messages.Count == 0) break;

            result.AddRange(list.Messages.Select(x => x.Id));
            pageToken = list.NextPageToken;
            if (string.IsNullOrEmpty(pageToken)) break;
        }

        return result;
    }

    public async Task<GmailMessageMetadata?> GetMessageAsync(string accessToken, string messageId, CancellationToken ct)
    {
        var url = $"messages/{Uri.EscapeDataString(messageId)}" +
                  "?format=metadata&metadataHeaders=From&metadataHeaders=Subject&metadataHeaders=List-Unsubscribe";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        var message = await res.Content.ReadFromJsonAsync<GmailMessage>(cancellationToken: ct);
        if (message is null)
        {
            return null;
        }

        var headers = message.Payload?.Headers?
            .Select(x => new GmailHeaderData(x.Name, x.Value))
            .ToArray() ?? Array.Empty<GmailHeaderData>();

        return new GmailMessageMetadata(
            message.Id,
            message.Snippet,
            message.InternalDate,
            headers);
    }
}
