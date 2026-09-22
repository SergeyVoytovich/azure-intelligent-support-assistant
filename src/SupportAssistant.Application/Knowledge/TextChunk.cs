namespace SupportAssistant.Application.Knowledge;

public record TextChunk
{
    public string Content { get; set; } = string.Empty;
    public int Index { get; set; }
    public string Source { get; set; } = string.Empty;
    public int? Page { get; set; }
}
