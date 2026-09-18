using Azure;
using Azure.AI.DocumentIntelligence;

namespace SupportAssistant.Infrastructure.Documents;

public static class DocumentIntelligenceClientExtensions
{
    public static Task<Operation<AnalyzeResult>> AnalyzePrebuiltAsync
        (this DocumentIntelligenceClient client, BinaryData data, CancellationToken cancellationToken = default)
        => client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", data, cancellationToken: cancellationToken);

    public static async Task<Operation<AnalyzeResult>> AnalyzePrebuiltAsync
        (this DocumentIntelligenceClient client, Stream stream, CancellationToken cancellationToken = default)
    {
        var data = await BinaryData.FromStreamAsync(stream, cancellationToken);
        return await client.AnalyzePrebuiltAsync(data, cancellationToken);
    }
}
