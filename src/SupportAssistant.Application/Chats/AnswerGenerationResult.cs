namespace SupportAssistant.Application.Chats;

public sealed record AnswerGenerationResult(string Answer, IReadOnlyCollection<string> Sources);
