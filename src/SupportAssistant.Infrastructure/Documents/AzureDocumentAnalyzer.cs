using Azure;
using Azure.AI.DocumentIntelligence;
using SupportAssistant.Application.Documents;

namespace SupportAssistant.Infrastructure.Documents;

public class AzureDocumentAnalyzer(DocumentIntelligenceClient client) : IDocumentAnalyzer
{
    protected virtual DocumentIntelligenceClient Client { get; } = client;

    public async Task<DocumentAnalysis> AnalyzeAsync(Stream document,  CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var data = await BinaryData.FromStreamAsync(document, cancellationToken);

        Operation<AnalyzeResult> operation = await Client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", data, cancellationToken);

        var result = operation.Value;

        return new DocumentAnalysis
        {
            Content = result.Content ?? string.Empty,
            PageCount = result.Pages.Count
        };
    }
}
