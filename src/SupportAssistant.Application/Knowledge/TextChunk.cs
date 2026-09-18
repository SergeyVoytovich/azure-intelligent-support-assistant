namespace SupportAssistant.Application.Knowledge;

public record TextChunk
{
    public string Content { get;set; }
    public int Index { get; set; }
    public string Source { get; set; }
    public int? Page { get; set; }
}
