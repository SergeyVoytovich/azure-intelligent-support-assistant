namespace SupportAssistant.Application.Language;

public interface ITextAnalyzer
{
    Task<TextAnalysisResult> AnalyzeAsync(string text, CancellationToken cancellationToken = default);
}
