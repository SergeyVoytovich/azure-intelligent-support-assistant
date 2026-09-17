namespace SupportAssistant.Application.Chats;

public sealed record ChatResult
    (
        string Answer,
        IReadOnlyCollection<string> Sources,
        bool EscalationRequired
    );
