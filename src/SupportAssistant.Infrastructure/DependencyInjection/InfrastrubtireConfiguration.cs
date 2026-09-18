namespace SupportAssistant.Infrastructure.DependencyInjection;

public record InfrastrubtireConfiguration
{
    public string DocumentsEndpoint { get; set; } = null!;
    public string SearchEndpoint { get; set; } = null!;
}
