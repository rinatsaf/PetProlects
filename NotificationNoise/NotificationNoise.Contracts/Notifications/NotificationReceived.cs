namespace Contracts.Notifications;

public sealed record NotificationReceived(
    Guid NotificationId,
    string UserId,
    string Provider,
    string ExternalMessageId,
    string FromEmail,
    string? FromName,
    string? Subject,
    string? Snippet,
    DateTimeOffset ReceivedAt,
    bool HasListUnsubscribe
);