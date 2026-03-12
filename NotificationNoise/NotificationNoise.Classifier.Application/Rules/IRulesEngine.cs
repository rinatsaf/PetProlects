using Contracts.Notifications;

namespace NotificationNoise.Classifier.Application.Rules;

public interface IRulesEngine
{
    RuleResult Evaluate(NotificationReceived n);
}