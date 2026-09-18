
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace SupportAssistant.Infrastructure.Search;

public class SearchIndexInitializer(SearchIndexClient client)
{
    protected  virtual SearchIndexClient Client { get; } = client;

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        var index = new SearchIndex(SearchIndexDefinition.IndexName, SearchIndexDefinition.Fields)
        {
            VectorSearch = new VectorSearch()
        };
        index.VectorSearch.Algorithms.Add(new HnswAlgorithmConfiguration("hnsw-config"));
        index.VectorSearch.Profiles.Add(new VectorSearchProfile("vector-profile", "hnsw-config"));

        await Client.CreateOrUpdateIndexAsync(index, cancellationToken: cancellationToken);
    }
}
