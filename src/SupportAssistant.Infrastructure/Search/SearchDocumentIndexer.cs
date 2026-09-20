using System.Runtime.CompilerServices;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using SupportAssistant.Application.Embeddings;
using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.Infrastructure.Search;

public class SearchDocumentIndexer(SearchClient searchClient, IEmbeddingGenerator generator)
{
    protected virtual SearchClient SearchClient { get; } = searchClient;
    protected virtual  IEmbeddingGenerator Generator { get;  } = generator;


    public async Task IndexAsync(IReadOnlyCollection<TextChunk> chunks, CancellationToken  cancellationToken = default)
    {
        if (chunks.Count == 0)
        {
            return;
        }

        var documents = await GenerateAsync(chunks, cancellationToken)
                            .ToListAsync(cancellationToken: cancellationToken);

        await SearchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(documents),  cancellationToken: cancellationToken);
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
}
