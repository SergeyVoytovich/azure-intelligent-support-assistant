namespace SupportAssistant.Application.Documents;

public record DocumentAnalysis
{
    public string Content { get; set; } = null!;
    public int PageCount { get; set; }
}
