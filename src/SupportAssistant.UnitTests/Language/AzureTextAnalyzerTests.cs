using Azure;
using Azure.AI.TextAnalytics;
using SupportAssistant.Infrastructure.Language;

namespace SupportAssistant.UnitTests.Language;

public sealed class AzureTextAnalyzerTests
{
    [Fact]
    public async Task AnalysisPreservesSentimentScoresAndKeyPhrases()
    {
        var client = new StubTextClient();
        using var cancellation = new CancellationTokenSource();
        var result = await new AzureTextAnalyzer(client).AnalyzeAsync("Where is my refund?", cancellation.Token);
        Assert.Equal("Negative", result.Sentiment);
        Assert.Equal(0.03, result.PositiveScore);
        Assert.Equal(0.07, result.NeutralScore);
        Assert.Equal(0.9, result.NegativeScore);
        Assert.Equal(new[] { "refund", "order" }, result.KeyPhrases);
        Assert.Equal(new[] { "sentiment", "phrases" }, client.Calls.Select(call => call.Operation));
        Assert.All(client.Calls, call =>
        {
            Assert.Equal("Where is my refund?", call.Text);
            Assert.Equal(cancellation.Token, call.Token);
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    public async Task InvalidTextDoesNotCallAzure(string? text)
    {
        var client = new StubTextClient();
        await Assert.ThrowsAnyAsync<ArgumentException>(() => new AzureTextAnalyzer(client).AnalyzeAsync(text!));
        Assert.Empty(client.Calls);
    }

    [Fact]
    public async Task SentimentFailurePreventsKeyPhraseRequest()
    {
        var failure = new RequestFailedException(503, "offline");
        var client = new StubTextClient { Failure = failure };
        Assert.Same(failure, await Assert.ThrowsAsync<RequestFailedException>(() => new AzureTextAnalyzer(client).AnalyzeAsync("question")));
        Assert.Equal("sentiment", Assert.Single(client.Calls).Operation);
    }

    private sealed class StubTextClient : TextAnalyticsClient
    {
        public Exception? Failure { get; init; }
        public List<(string Operation, string Text, CancellationToken Token)> Calls { get; } = [];

        public override Task<Response<DocumentSentiment>> AnalyzeSentimentAsync(string document, string? language = null, AnalyzeSentimentOptions? options = null, CancellationToken cancellationToken = default)
        {
            Calls.Add(("sentiment", document, cancellationToken));
            return Failure is null
                ? Task.FromResult(Response.FromValue(TextAnalyticsModelFactory.DocumentSentiment(TextSentiment.Negative, 0.03, 0.07, 0.9, [], []), null!))
                : Task.FromException<Response<DocumentSentiment>>(Failure);
        }

        public override Task<Response<KeyPhraseCollection>> ExtractKeyPhrasesAsync(string document, string? language = null, CancellationToken cancellationToken = default)
        {
            Calls.Add(("phrases", document, cancellationToken));
            return Task.FromResult(Response.FromValue(TextAnalyticsModelFactory.KeyPhraseCollection(["refund", "order"], []), null!));
        }
    }
}
