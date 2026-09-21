using SupportAssistant.Application.Language;

namespace SupportAssistant.Application.Escalation;

public sealed class EscalationPolicy : IEscalationPolicy
{
    public EscalationDecision Evaluate(string message, TextAnalysisResult analysis)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var evaluation = FindEvaluation(message.ToLowerInvariant());
        if (evaluation is not null)
        {
            return evaluation;
        }

        evaluation = FindEvaluation(analysis.KeyPhrases);
        if (evaluation is not null)
        {
            return evaluation;
        }

        return analysis.NegativeScore >= 0.85
            ? new EscalationDecision(true, "Highly negative customer sentiment.")
            : new EscalationDecision(false, null);
    }

    private EscalationDecision? FindEvaluation(IEnumerable<string> messages)
        => messages.Select(FindEvaluation).FirstOrDefault();

    private EscalationDecision? FindEvaluation(string message)
        => CriticalPhrases
            .Values
            .Where(phrase => message.Contains(phrase, StringComparison.OrdinalIgnoreCase))
            .Select(phrase => new EscalationDecision(true, $"Critical phrase detected: {phrase}"))
            .FirstOrDefault();
}
