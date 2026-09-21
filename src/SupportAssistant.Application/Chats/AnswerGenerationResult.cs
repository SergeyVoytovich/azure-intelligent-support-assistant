namespace SupportAssistant.Application.Chats;

public sealed record AnswerGenerationResult
{
    public string Answer { get; init; }

    public IReadOnlyCollection<string> Sources{ get; init; }
}
