using Azure.AI.TextAnalytics;
using SupportAssistant.Application.Language;

namespace SupportAssistant.Infrastructure.Language;

public class AzureTextAnalyzer(TextAnalyticsClient client) : ITextAnalyzer
{
    protected virtual TextAnalyticsClient Client { get; } = client;

    public async Task<TextAnalysisResult> AnalyzeAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var sentimentResponse = await Client.AnalyzeSentimentAsync(text, cancellationToken: cancellationToken);
        var sentiment = sentimentResponse.Value;

        var keyPhrasesResponse = await Client.ExtractKeyPhrasesAsync(text, cancellationToken: cancellationToken);

        return new TextAnalysisResult(
            Sentiment: sentiment.Sentiment.ToString(),
            PositiveScore: sentiment.ConfidenceScores.Positive,
            NeutralScore: sentiment.ConfidenceScores.Neutral,
            NegativeScore: sentiment.ConfidenceScores.Negative,
            KeyPhrases: keyPhrasesResponse.Value.ToArray());
    }
}
