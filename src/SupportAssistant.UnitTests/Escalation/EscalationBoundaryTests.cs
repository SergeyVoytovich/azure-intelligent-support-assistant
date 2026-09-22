using SupportAssistant.Application.Escalation;
using SupportAssistant.Application.Language;

namespace SupportAssistant.UnitTests.Escalation;

public sealed class EscalationBoundaryTests
{
    [Theory]
    [InlineData(0.849999, false)]
    [InlineData(0.85, true)]
    [InlineData(1, true)]
    public void NegativeSentimentThresholdIsInclusive(double score, bool expected)
    {
        var result = new EscalationPolicy().Evaluate("Where is my order?", new TextAnalysisResult("Negative", 0, 0, score, []));
        Assert.Equal(expected, result.Required);
        Assert.Equal(expected ? "Highly negative customer sentiment." : null, result.Reason);
    }

    [Theory]
    [InlineData("UNAUTHORIZED PURCHASE", "unauthorized purchase")]
    [InlineData("unauthorized transaction", "unauthorized transaction")]
    [InlineData("fraudulent transaction", "fraud")]
    [InlineData("ACCOUNT TAKEOVER", "account takeover")]
    [InlineData("account hacked", "account hacked")]
    [InlineData("stolen account", "stolen account")]
    [InlineData("refund not received", "refund not received")]
    [InlineData("refund has not arrived", "refund has not arrived")]
    [InlineData("repeated product failure", "repeated product failure")]
    [InlineData("disputed warranty", "disputed warranty")]
    public void CriticalPhrasesTakePriorityOverSentiment(string phrase, string match)
    {
        var result = new EscalationPolicy().Evaluate($"Please help with {phrase} today", new TextAnalysisResult("Negative", 0, 0, 1, []));
        Assert.True(result.Required);
        Assert.Equal($"Critical phrase detected: {match}", result.Reason);
    }

    [Fact]
    public void CriticalExtractedPhraseTriggersEscalation()
    {
        var result = new EscalationPolicy().Evaluate("Please help", new TextAnalysisResult("Neutral", 0, 1, 0, ["ACCOUNT HACKED"]));
        Assert.True(result.Required);
        Assert.Equal("Critical phrase detected: account hacked", result.Reason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public void InvalidMessageIsRejected(string? message)
        => Assert.ThrowsAny<ArgumentException>(() => new EscalationPolicy().Evaluate(message!, new TextAnalysisResult("Neutral", 0, 1, 0, [])));
}
