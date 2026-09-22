using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using SupportAssistant.Application.Documents;
using SupportAssistant.Application.Knowledge;
using SupportAssistant.Infrastructure.Search;

namespace SupportAssistant.Infrastructure.Ingestion;

public class KnowledgeIngestionService(
    BlobContainerClient blobContainerClient,
    IDocumentAnalyzer documentAnalyzer,
    ITextChunker textChunker,
    SearchDocumentIndexer documentIndexer,
    SearchIndexInitializer indexInitializer,
    ILogger<SearchDocumentIndexer> logger)
{
    private static readonly Action<ILogger, string, int, int, Exception?> LogBlobProcessed =
        LoggerMessage.Define<string, int, int>(
            LogLevel.Warning,
            new EventId(2001, nameof(RunAsync)),
            "Blob {BlobName}: extracted {CharacterCount} chars, created {ChunkCount} chunks.");

    protected virtual BlobContainerClient BlobContainerClient { get; } = blobContainerClient;
    protected virtual IDocumentAnalyzer DocumentAnalyzer { get; } = documentAnalyzer;
    protected virtual ITextChunker TextChunker { get; } = textChunker;
    protected virtual SearchDocumentIndexer DocumentIndexer { get; } = documentIndexer;
    protected virtual SearchIndexInitializer IndexInitializer { get; } = indexInitializer;
    protected ILogger<SearchDocumentIndexer> Logger { get; } = logger;

    public async Task<IngestionResult> RunAsync(
        CancellationToken cancellationToken = default)
    {
        await IndexInitializer.EnsureCreatedAsync(cancellationToken);

        var blobCount = 0;
        var pdfCount = 0;
        var chunkCount = 0;
        var blobNames = new List<string>();

        await foreach (
            var blobItem in BlobContainerClient.GetBlobsAsync(
                cancellationToken: cancellationToken))
        {
            var result = await RunAsync(blobItem, cancellationToken);

            blobCount += result.BlobCount;
            pdfCount += result.PdfCount;
            chunkCount += result.ChunkCount;
            blobNames.Add(result.BlobNames.Single());
        }

        return new IngestionResult(
            blobCount,
            pdfCount,
            chunkCount,
            blobNames);
    }

    protected virtual async Task<IngestionResult> RunAsync(
        BlobItem blobItem,
        CancellationToken cancellationToken = default)
    {
        if (!blobItem.Name.EndsWith(
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return new IngestionResult(
                1,
                0,
                0,
                [blobItem.Name]);
        }

        var blobClient =
            BlobContainerClient.GetBlobClient(blobItem.Name);

        await using var stream =
            await blobClient.OpenReadAsync(
                cancellationToken: cancellationToken);

        var analysis =
            await DocumentAnalyzer.AnalyzeAsync(
                stream,
                cancellationToken);

        var chunks =
            TextChunker.Chunk(
                analysis.Content,
                blobItem.Name);

        Console.WriteLine(
            $"Blob {blobItem.Name}: {analysis.Content.Length} chars, {chunks.Count} chunks.");

        LogBlobProcessed(
            Logger,
            blobItem.Name,
            analysis.Content.Length,
            chunks.Count,
            null);

        await DocumentIndexer.IndexAsync(
            chunks,
            cancellationToken);

        return new IngestionResult(
            1,
            1,
            chunks.Count,
            [blobItem.Name]);
    }
}
