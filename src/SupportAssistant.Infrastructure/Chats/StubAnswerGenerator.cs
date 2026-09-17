using SupportAssistant.Application.Chats;

namespace SupportAssistant.Infrastructure.Chats;

public sealed class StubAnswerGenerator : IAnswerGenerator
{
    public Task<string> GenerateAsync(string question, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"AI integration is not configured yet. Question: {question}");
    }
}
