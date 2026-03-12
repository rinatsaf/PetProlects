using Contracts.Notifications;

namespace NotificationNoise.Classifier.Application.Rules;

public sealed class SimpleRulesEngine : IRulesEngine
{
    public RuleResult Evaluate(NotificationReceived n)
    {
        var reasons = new List<string>();
        var score = 0;

        if (n.HasListUnsubscribe)
        {
            score += 30;
            reasons.Add("has_list_unsubscribe");
        }

        if (n.FromEmail.Contains("no-reply", StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
            reasons.Add("no_reply_sender");
        }

        var subj = n.Subject ?? "";
        if (subj.Contains("sale", StringComparison.OrdinalIgnoreCase) ||
            subj.Contains("offer", StringComparison.OrdinalIgnoreCase) ||
            subj.Contains("скид", StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
            reasons.Add("promo_subject");
        }

        if (n.FromEmail.EndsWith("@shop.com", StringComparison.OrdinalIgnoreCase))
        {
            score += 20;
            reasons.Add("promo_domain");
        }

        score = Math.Clamp(score, 0, 100);
        var label = score >= 50 ? "noise" : "useful";

        return new RuleResult(label, score, reasons.ToArray());
    }
}
