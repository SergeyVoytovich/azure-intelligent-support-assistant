using System.Text.Json.Serialization;

namespace SupportAssistant.Infrastructure.Search;

public class SearchDocument
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("contentVector")]
    public required IReadOnlyList<float> ContentVector { get; init; }

    [JsonPropertyName("source")]
    public required string Source { get; init; }

    [JsonPropertyName("pageNumber")]
    public int? PageNumber { get; init; }

    [JsonPropertyName("chunkIndex")]
    public int ChunkIndex { get; init; }
}
