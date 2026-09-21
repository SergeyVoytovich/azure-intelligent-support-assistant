using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Escalation;
using SupportAssistant.Application.Language;

namespace SupportAssistant.UnitTests.Chats;

public sealed class ChatServiceBehaviorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ServicePassesOriginalInputsAndPreservesAnswerAndSources(bool escalate)
    {
        var dependencies = new Dependencies { Escalate = escalate };
        var service = new ChatService(dependencies, dependencies, dependencies);
        using var cancellation = new CancellationTokenSource();
        var result = await service.HandleAsync(new ChatCommand("My question"), cancellation.Token);
        Assert.Equal("answer", result.Answer);
        Assert.Same(dependencies.Answer.Sources, result.Sources);
        Assert.Equal(escalate, result.EscalationRequired);
        Assert.Equal(new[] { "analyze", "evaluate", "generate" }, dependencies.Calls);
        Assert.All(dependencies.Texts, text => Assert.Equal("My question", text));
        Assert.All(dependencies.Tokens, token => Assert.Equal(cancellation.Token, token));
        Assert.Same(dependencies.Analysis, dependencies.EvaluatedAnalysis);
    }

    [Fact]
    public async Task NullCommandDoesNotInvokeDependencies()
    {
        var dependencies = new Dependencies();
        await Assert.ThrowsAsync<ArgumentNullException>("command", () => new ChatService(dependencies, dependencies, dependencies).HandleAsync(null!));
        Assert.Empty(dependencies.Calls);
    }

    [Theory]
    [InlineData("analyze", 1)]
    [InlineData("evaluate", 2)]
    [InlineData("generate", 3)]
    public async Task DependencyFailurePropagatesAndStopsPipeline(string step, int callCount)
    {
        var dependencies = new Dependencies { FailAt = step };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => new ChatService(dependencies, dependencies, dependencies).HandleAsync(new ChatCommand("question")));
        Assert.Same(dependencies.Failure, error);
        Assert.Equal(callCount, dependencies.Calls.Count);
        Assert.Equal(step, dependencies.Calls[^1]);
    }

    [Fact]
    public void ChatResultFactoriesPreserveValuesAndUseEmptySourcesByDefault()
    {
        var plain = ChatResult.New("answer");
        Assert.Equal("answer", plain.Answer);
        Assert.False(plain.EscalationRequired);
        Assert.Empty(plain.Sources);
        var escalated = ChatResult.New("answer", true);
        Assert.True(escalated.EscalationRequired);
        Assert.Empty(escalated.Sources);
        string[] sources = ["a.pdf", "b.pdf"];
        var sourced = ChatResult.New("answer", sources, true);
        Assert.Same(sources, sourced.Sources);
        Assert.Equal("answer", sourced.Answer);
        Assert.True(sourced.EscalationRequired);
    }

    private sealed class Dependencies : IAnswerGenerator, ITextAnalyzer, IEscalationPolicy
    {
        public bool Escalate { get; init; }
        public string? FailAt { get; init; }
        public InvalidOperationException Failure { get; } = new("dependency failed");
        public List<string> Calls { get; } = [];
        public List<string> Texts { get; } = [];
        public List<CancellationToken> Tokens { get; } = [];
        public TextAnalysisResult Analysis { get; } = new("Neutral", 0.1, 0.8, 0.1, ["question"]);
        public TextAnalysisResult? EvaluatedAnalysis { get; private set; }
        public AnswerGenerationResult Answer { get; } = new("answer", ["a.pdf", "b.pdf"]);

        private void Record(string operation, string text)
        {
            Calls.Add(operation);
            Texts.Add(text);
            if (FailAt == operation) throw Failure;
        }
        public Task<TextAnalysisResult> AnalyzeAsync(string text, CancellationToken cancellationToken = default)
        {
            Tokens.Add(cancellationToken);
            Record("analyze", text);
            return Task.FromResult(Analysis);
        }
        public EscalationDecision Evaluate(string message, TextAnalysisResult analysis)
        {
            EvaluatedAnalysis = analysis;
            Record("evaluate", message);
            return new(Escalate, Escalate ? "reason" : null);
        }
        public Task<AnswerGenerationResult> GenerateAsync(string question, CancellationToken cancellationToken = default)
        {
            Tokens.Add(cancellationToken);
            Record("generate", question);
            return Task.FromResult(Answer);
        }
    }
}
