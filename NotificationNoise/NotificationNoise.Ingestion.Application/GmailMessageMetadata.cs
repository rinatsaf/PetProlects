namespace NotificationNoise.Ingestion.Application;

public sealed record GmailMessageMetadata(
    string Id,
    string? Snippet,
    string? InternalDate,
    IReadOnlyList<GmailHeaderData> Headers);

public sealed record GmailHeaderData(string Name, string Value);
