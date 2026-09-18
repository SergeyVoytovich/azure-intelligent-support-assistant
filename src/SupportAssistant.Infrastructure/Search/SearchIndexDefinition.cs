namespace SupportAssistant.Infrastructure.Search;

public class SearchIndexDefinition
{
    public const string IndexName = "knowledge-index";

    public const string IdField = "id";
    public const string ContentField = "content";
    public const string ContentVectorField = "contentVector";
    public const string SourceField = "source";
    public const string PageNumberField = "pageNumber";
    public const string ChunkIndexField = "chunkIndex";

    public const int EmbeddingDimensions = 1536;
}
