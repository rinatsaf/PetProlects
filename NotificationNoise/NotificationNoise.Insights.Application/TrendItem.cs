namespace NotificationNoise.Insights.Application;

public sealed record TrendItem(
    DateTime Date,
    int Total,
    int Noise,
    int Useful);
