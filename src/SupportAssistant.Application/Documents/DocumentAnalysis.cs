namespace SupportAssistant.Application.Documents;

public record DocumentAnalysis
{
    public string Contenst { get; set; } = null!;
    public int PageCount { get; set; }
}
