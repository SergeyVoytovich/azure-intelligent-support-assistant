namespace SupportAssistant.Application.Language;

public sealed record TextAnalysisResult(
    string Sentiment,
    double PositiveScore,
    double NeutralScore,
    double NegativeScore,
    IReadOnlyCollection<string> KeyPhrases);
