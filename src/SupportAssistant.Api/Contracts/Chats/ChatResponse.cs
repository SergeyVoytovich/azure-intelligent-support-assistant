namespace SupportAssistant.Api.Contracts.Chats;

public record ChatResponse
{
    public string Answer { get; set; } = null!;
    public IReadOnlyCollection<string> Sources { get; set; } = [];
    public bool EscalationRequired { get; set; }
    public string RequestId { get; set; } = null!;
}
