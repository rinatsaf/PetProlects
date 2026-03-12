using System.Text.Json.Serialization;

namespace NotificationNoise.Ingestion.Infrastructure;

public sealed class GmailListResponse
{
    [JsonPropertyName("messages")]
    public List<GmailMessageRef>? Messages { get; init; }

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; init; }
}

public sealed class GmailMessageRef
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = default!;
}

public sealed class GmailMessage
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = default!;

    [JsonPropertyName("snippet")]
    public string? Snippet { get; init; }

    [JsonPropertyName("internalDate")]
    public string? InternalDate { get; init; }

    [JsonPropertyName("payload")]
    public GmailPayload? Payload { get; init; }
}

public sealed class GmailPayload
{
    [JsonPropertyName("headers")]
    public List<GmailHeader>? Headers { get; init; }
}

public sealed class GmailHeader
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("value")]
    public string Value { get; init; } = default!;
}
