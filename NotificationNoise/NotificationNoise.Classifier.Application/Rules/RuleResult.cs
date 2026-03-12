namespace NotificationNoise.Classifier.Application.Rules;

public sealed record RuleResult(string Label, int Score, string[] Reasons);