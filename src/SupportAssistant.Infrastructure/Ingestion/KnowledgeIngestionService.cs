using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SupportAssistant.Application.Documents;
using SupportAssistant.Application.Knowledge;
using SupportAssistant.Infrastructure.Search;

namespace SupportAssistant.Infrastructure.Ingestion;

public class KnowledgeIngestionService(
    BlobContainerClient blobContainerClient,
    IDocumentAnalyzer documentAnalyzer,
    ITextChunker textChunker,
    SearchDocumentIndexer documentIndexer)
{
    protected virtual BlobContainerClient BlobContainerClient { get; } = blobContainerClient;
    protected virtual IDocumentAnalyzer DocumentAnalyzer { get; } = documentAnalyzer;
    protected virtual ITextChunker TextChunker { get; } = textChunker;
    protected virtual SearchDocumentIndexer DocumentIndexer { get; } = documentIndexer;


    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var blobItem in BlobContainerClient.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            await RunAsync(blobItem, cancellationToken);
        }
    }

    protected virtual async Task RunAsync(BlobItem blobItem, CancellationToken cancellationToken = default)
    {
        if (!blobItem.Name.EndsWith(".pfd", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var blobClient = BlobContainerClient.GetBlobClient(blobItem.Name);

        await using var stream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);

        var analysis = await DocumentAnalyzer.AnalyzeAsync(stream, cancellationToken);

        var chunks = TextChunker.Chunk(analysis.Content, blobItem.Name);

        await DocumentIndexer.IndexAsync(chunks, cancellationToken);
    }
}
