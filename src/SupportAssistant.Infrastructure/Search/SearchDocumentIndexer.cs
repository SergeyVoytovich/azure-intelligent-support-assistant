using System.Runtime.CompilerServices;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using SupportAssistant.Application.Embeddings;
using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.Infrastructure.Search;

public class SearchDocumentIndexer(SearchClient searchClient, IEmbeddingGenerator generator, ILogger<SearchDocumentIndexer> logger)
{
    protected virtual SearchClient SearchClient { get; } = searchClient;
    protected virtual  IEmbeddingGenerator Generator { get;  } = generator;
    protected ILogger<SearchDocumentIndexer> Logger { get; } = logger;


    public async Task IndexAsync(IReadOnlyCollection<TextChunk> chunks, CancellationToken  cancellationToken = default)
    {
        if (chunks.Count == 0)
        {
            return;
        }

        var documents = await GenerateAsync(chunks, cancellationToken)
                            .ToListAsync(cancellationToken: cancellationToken);

        logger.LogWarning("Uploading {Count} documents to Azure AI Search.", documents.Count);
        Console.WriteLine($"Uploading {documents.Count} documents to Azure AI Search.");

        var response = await SearchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(documents),  cancellationToken: cancellationToken);
        LogResponse(response);
    }

    protected virtual async IAsyncEnumerable<SearchDocument> GenerateAsync
        (IReadOnlyCollection<TextChunk> chunks, [EnumeratorCancellation] CancellationToken  cancellationToken = default)
    {
        foreach (var chunk in chunks)
        {
            var embedding = await Generator.GenerateAsync(chunk.Content, cancellationToken);

            yield return new SearchDocument
            {
                Id = $"{Sanitize(chunk.Source)}-{chunk.Index}",
                Content = chunk.Content,
                ContentVector = embedding,
                Source = chunk.Source,
                PageNumber = chunk.Page,
                ChunkIndex = chunk.Index
            };
        }
    }

    private static string Sanitize(string value)
        => Path.GetFileNameWithoutExtension(value)
                .Replace(' ', '-')
                .ToLowerInvariant();

    protected virtual void LogResponse(Response<IndexDocumentsResult> response)
    {
        foreach (var result in response.Value.Results)
        {
            Console.WriteLine($"Index result: {result.Key}, success={result.Succeeded}, status={result.Status}, error={result.ErrorMessage}");
            logger.LogWarning(
                "Index result: Key={Key}, Succeeded={Succeeded}, Status={Status}, Error={Error}",
                result.Key,
                result.Succeeded,
                result.Status,
                result.ErrorMessage);
        }
    }
}
