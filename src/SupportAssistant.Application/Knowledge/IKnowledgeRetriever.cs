namespace SupportAssistant.Application.Knowledge;

public interface IKnowledgeRetriever
{
    Task<IReadOnlyCollection<KnowledgeSearchResult>> SearchAsync(string query, int top = 5, CancellationToken cancellationToken = default);
}
