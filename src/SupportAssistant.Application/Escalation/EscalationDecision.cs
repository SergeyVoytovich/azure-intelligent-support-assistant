namespace SupportAssistant.Application.Escalation;

public sealed record EscalationDecision(
    bool Required,
    string? Reason);
