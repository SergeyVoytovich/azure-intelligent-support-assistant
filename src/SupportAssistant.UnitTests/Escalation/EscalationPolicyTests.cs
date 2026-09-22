using System.Diagnostics.CodeAnalysis;
using SupportAssistant.Application.Escalation;
using SupportAssistant.Application.Language;

namespace SupportAssistant.UnitTests.Escalation;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public sealed class EscalationPolicyTests
{
    private readonly EscalationPolicy _sut = new();

    [Fact]
    public void Evaluate_ShouldEscalateUnauthorizedPurchase()
    {
        var analysis = CreateAnalysis();

        EscalationDecision result =
            _sut.Evaluate(
                "There is an unauthorized purchase on my account.",
                analysis);

        Assert.True(result.Required);
    }

    [Fact]
    public void Evaluate_ShouldEscalateAccountTakeover()
    {
        var analysis = CreateAnalysis();

        EscalationDecision result =
            _sut.Evaluate(
                "I think this is an account takeover.",
                analysis);

        Assert.True(result.Required);
    }

    [Fact]
    public void Evaluate_ShouldEscalateHighlyNegativeSentiment()
    {
        var analysis = CreateAnalysis(
            negativeScore: 0.92);

        EscalationDecision result =
            _sut.Evaluate(
                "I am extremely unhappy with this service.",
                analysis);

        Assert.True(result.Required);
    }

    [Fact]
    public void Evaluate_ShouldNotEscalateNormalQuestion()
    {
        var analysis = CreateAnalysis(
            positiveScore: 0.8,
            negativeScore: 0.05);

        EscalationDecision result =
            _sut.Evaluate(
                "How long is the warranty?",
                analysis);

        Assert.False(result.Required);
    }

    private static TextAnalysisResult CreateAnalysis(
        double positiveScore = 0.1,
        double neutralScore = 0.8,
        double negativeScore = 0.1,
        IReadOnlyCollection<string>? keyPhrases = null)
        => new(
            Sentiment: "Neutral",
            PositiveScore: positiveScore,
            NeutralScore: neutralScore,
            NegativeScore: negativeScore,
            KeyPhrases: keyPhrases ?? []);
}
