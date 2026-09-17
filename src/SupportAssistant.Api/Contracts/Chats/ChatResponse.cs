namespace SupportAssistant.Api.Contracts.Chats;

public record ChatResponse(
    string Answer,
    IReadOnlyCollection<string> Sources,
    bool EscalationRequired,
    string RequestId);
