namespace Contracts.Notifications;

public record NotificationClassified(
    Guid NotificationId,
    string UserId,
    string Provider,
    string ExternalMessageId,
    string FromEmail,
    string? FromName,
    DateTimeOffset ReceivedAt,
    bool HasListUnsubscribe,
    string Label,
    int Score,
    string[] Reasons,
    DateTimeOffset ClassifiedAt
);
