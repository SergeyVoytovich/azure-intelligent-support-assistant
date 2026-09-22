using Azure.Search.Documents.Indexes.Models;

namespace SupportAssistant.Infrastructure.Search;

public static class SearchIndexDefinition
{
    public const string IndexName = "knowledge-index";

    public static IReadOnlyCollection<SearchField> Fields => [.. GetFields()];

    public const string IdField = "id";
    public const string ContentField = "content";
    public const string ContentVectorField = "contentVector";
    public const string SourceField = "source";
    public const string PageNumberField = "pageNumber";
    public const string ChunkIndexField = "chunkIndex";

    public const int EmbeddingDimensions = 1536;

    public const string SemanticConfigurationName = "semantic-config";

    private static IEnumerable<SearchField> GetFields()
    {
        yield return new SearchField(IdField, SearchFieldDataType.String)
        {
            IsKey = true,
            IsFilterable = true
        };

        yield return new SearchField(ContentField, SearchFieldDataType.String)
        {
            IsSearchable = true
        };

        yield return new SearchField(ContentVectorField, SearchFieldDataType.Collection(SearchFieldDataType.Single))
        {
            IsSearchable = true,
            VectorSearchDimensions = EmbeddingDimensions,
            VectorSearchProfileName = "vector-profile"
        };

        yield return new SearchField(SourceField, SearchFieldDataType.String)
        {
            IsFilterable = true,
            IsFacetable = true
        };

        yield return new SearchField(PageNumberField, SearchFieldDataType.Int32)
        {
            IsFilterable = true
        };

        yield return new SearchField(ChunkIndexField, SearchFieldDataType.Int32)
        {
            IsFilterable = true
        };
    }
}
