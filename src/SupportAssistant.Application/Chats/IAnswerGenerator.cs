namespace SupportAssistant.Application.Chats;

public interface IAnswerGenerator
{
    Task<AnswerGenerationResult> GenerateAsync(string question, CancellationToken  cancellationToken = default);
}
