using SupportAssistant.Application.Chats;

namespace SupportAssistant.UnitTests.Chats;

public sealed class ChatServiceTests
{
    [Fact]
    public async Task HandleAsync_WithValidQuestion_ReturnsGeneratedAnswer()
    {
        var answerGenerator = new FakeAnswerGenerator("Test answer");
        var chatService = new ChatService(answerGenerator);

        var result = await chatService.HandleAsync(new ChatCommand("Test question"));

        Assert.Equal("Test answer", result.Answer);
        Assert.Empty(result.Sources);
        Assert.False(result.EscalationRequired);
    }

    private sealed class FakeAnswerGenerator(string answer) : IAnswerGenerator
    {
        public Task<string> GenerateAsync(string question, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(answer);
        }
    }
}
