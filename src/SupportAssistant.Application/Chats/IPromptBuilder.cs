using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.Application.Chats;

public interface IPromptBuilder
{
    string Build(string userQuestion, IReadOnlyCollection<KnowledgeSearchResult> knowledge);
}
