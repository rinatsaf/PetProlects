namespace NotificationNoise.Classifier.Domain;

public sealed class Classification
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string UserId { get; private set; } = default!;
    public string Provider { get; private set; } = default!;
    public string ExternalMessageId { get; private set; } = default!;
    public Guid NotificationId { get; private set; }

    public string Label { get; private set; } = default!;
    public int Score { get; private set; }
    public string ReasonsJson { get; private set; } = "[]";
    public DateTimeOffset ClassifiedAt { get; private set; }

    private Classification() { }

    public Classification(
        Guid notificationId,
        string userId,
        string provider,
        string externalMessageId,
        string label,
        int score,
        string reasonsJson,
        DateTimeOffset classifiedAt)
    {
        NotificationId = notificationId;
        UserId = userId;
        Provider = provider;
        ExternalMessageId = externalMessageId;

        Label = label;
        Score = score;
        ReasonsJson = reasonsJson;
        ClassifiedAt = classifiedAt;
    }
}