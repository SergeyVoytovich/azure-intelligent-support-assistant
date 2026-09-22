namespace SupportAssistant.Application.Knowledge;

public sealed record KnowledgeSearchResult(
    string Content,
    string Source,
    int? PageNumber,
    double Score);
