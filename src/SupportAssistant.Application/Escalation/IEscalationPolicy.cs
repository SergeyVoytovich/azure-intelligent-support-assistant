using SupportAssistant.Application.Language;

namespace SupportAssistant.Application.Escalation;

public interface IEscalationPolicy
{
    EscalationDecision Evaluate(string message, TextAnalysisResult analysis);
}
