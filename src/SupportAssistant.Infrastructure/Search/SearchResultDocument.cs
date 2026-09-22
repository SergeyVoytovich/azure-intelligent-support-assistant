using System.Text.Json.Serialization;

namespace SupportAssistant.Infrastructure.Search;

public sealed class SearchResultDocument
{
    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("pageNumber")]
    public int? PageNumber { get; init; }
}
