namespace SupportAssistant.Application.Documents;

public interface IDocumentAnalyzer
{
    Task<DocumentAnalysis> AnalyzeAsync(Stream stream, CancellationToken cancellationToken = default);
}
