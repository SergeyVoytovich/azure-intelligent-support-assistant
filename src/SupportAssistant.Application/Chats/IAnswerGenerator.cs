namespace SupportAssistant.Application.Chats;

public interface IAnswerGenerator
{
    Task<string> GenerateAsync(string question, CancellationToken  cancellationToken = default);
}
