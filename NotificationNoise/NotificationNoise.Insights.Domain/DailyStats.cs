namespace NotificationNoise.Insights.Domain;

public sealed class DailyStats
{
    public string UserId { get; private set; } = default!;
    public DateTime Date { get; private set; }
    public int Total { get; private set; }
    public int Noise { get; private set; }
    public int Useful { get; private set; }

    private DailyStats() { }

    public DailyStats(string userId, DateTime date, bool isNoise)
    {
        UserId = userId;
        Date = date;
        ApplyNotification(isNoise);
    }

    public void ApplyNotification(bool isNoise)
    {
        Total++;
        if (isNoise) Noise++; else Useful++;
    }
}
