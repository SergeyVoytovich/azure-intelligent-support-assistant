using System.Diagnostics.CodeAnalysis;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Escalation;
using SupportAssistant.Application.Language;

namespace SupportAssistant.UnitTests.Chats;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public sealed class ChatServiceTests
{
    [Fact]
    public async Task HandleAsync_WithValidQuestion_ReturnsGeneratedAnswer()
    {
        var answerGenerator = new FakeAnswerGenerator("Test answer");

        var textAnalyzer = new FakeTextAnalyzer(
            new TextAnalysisResult(
                Sentiment: "Neutral",
                PositiveScore: 0.1,
                NeutralScore: 0.8,
                NegativeScore: 0.1,
                KeyPhrases: []));

        var escalationPolicy = new FakeEscalationPolicy(new EscalationDecision(Required: false, Reason: null));

        var chatService = new ChatService(answerGenerator, textAnalyzer, escalationPolicy);

        var result = await chatService.HandleAsync(new ChatCommand("Test question"));

        Assert.Equal("Test answer", result.Answer);
        Assert.Empty(result.Sources);
        Assert.False(result.EscalationRequired);
    }

    private sealed class FakeAnswerGenerator(string answer) : IAnswerGenerator
    {
        public Task<AnswerGenerationResult> GenerateAsync(string question,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new AnswerGenerationResult(Answer: answer, Sources: []));
    }

    private sealed class FakeTextAnalyzer(TextAnalysisResult result) : ITextAnalyzer
    {
        public Task<TextAnalysisResult> AnalyzeAsync(string text, CancellationToken cancellationToken = default) =>
            Task.FromResult(result);
    }

    private sealed class FakeEscalationPolicy(EscalationDecision decision) : IEscalationPolicy
    {
        public EscalationDecision Evaluate(string message, TextAnalysisResult analysis) => decision;
    }

    [Fact]
    public async Task HandleAsync_WhenEscalationRequired_ReturnsEscalationFlag()
    {
        var answerGenerator = new FakeAnswerGenerator("Test answer");

        var textAnalyzer = new FakeTextAnalyzer(
                            new TextAnalysisResult(
                                Sentiment: "Negative",
                                PositiveScore: 0.01,
                                NeutralScore: 0.04,
                                NegativeScore: 0.95,
                                KeyPhrases: []));

        var escalationPolicy = new FakeEscalationPolicy(new EscalationDecision(Required: true, Reason: "Highly negative customer sentiment."));

        var chatService = new ChatService(answerGenerator, textAnalyzer, escalationPolicy);

        var result = await chatService.HandleAsync(new ChatCommand("This service is terrible."));

        Assert.True(result.EscalationRequired);
    }
}
