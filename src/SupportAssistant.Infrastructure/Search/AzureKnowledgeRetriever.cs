using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using SupportAssistant.Application.Embeddings;
using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.Infrastructure.Search;

public class AzureKnowledgeRetriever(SearchClient searchClient, IEmbeddingGenerator embeddingGenerator) : IKnowledgeRetriever
{
    protected virtual IEmbeddingGenerator EmbeddingGenerator { get; } = embeddingGenerator;
    protected virtual SearchClient Client { get; } = searchClient;

    public async Task<IReadOnlyCollection<KnowledgeSearchResult>> SearchAsync(string query, int top = 5, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var options = await GetSearchOptionsAsync(query, top, cancellationToken);
        return await SearchAsync(query, options, cancellationToken);
    }

    protected virtual async Task<IReadOnlyCollection<KnowledgeSearchResult>> SearchAsync(string query, SearchOptions options, CancellationToken cancellationToken = default)
    {
        var response = await Client.SearchAsync<SearchResultDocument>(query, options, cancellationToken);

        var results = new List<KnowledgeSearchResult>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            results.Add(
                new KnowledgeSearchResult(
                    result.Document.Content ?? string.Empty,
                    result.Document.Source ?? string.Empty,
                    result.Document.PageNumber,
                    result.Score ?? 0));
        }

        return results;
    }

    protected virtual async Task<SearchOptions> GetSearchOptionsAsync(string query, int top = 5, CancellationToken cancellationToken = default)
    {
         var queryVector = await EmbeddingGenerator.GenerateAsync(query, cancellationToken);

         var options = new SearchOptions
         {
             Size = top,
             QueryType = SearchQueryType.Semantic,
             SemanticSearch = GetSemanticSearchOptions(),
             VectorSearch = GetVectorSearchOptions(queryVector, top)
         };

         options.Select.Add(SearchIndexDefinition.ContentField);
         options.Select.Add(SearchIndexDefinition.SourceField);
         options.Select.Add(SearchIndexDefinition.PageNumberField);

         return options;
    }

    protected virtual SemanticSearchOptions GetSemanticSearchOptions()
        => new()
        {
            SemanticConfigurationName =
                SearchIndexDefinition.SemanticConfigurationName
        };

    protected virtual VectorSearchOptions GetVectorSearchOptions(IReadOnlyList<float> queryVector, int top)
        => new()
        {
            Queries =
            {
                new VectorizedQuery(queryVector.ToArray())
                {
                    KNearestNeighborsCount = top,
                    Fields =
                    {
                        SearchIndexDefinition.ContentVectorField
                    }
                }
            }
        };
}
