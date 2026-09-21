using OpenAI.Chat;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.Infrastructure.Chats;

public class AzureOpenAiAnswerGenerator(
    ChatClient chatClient, IKnowledgeRetriever retriever, IPromptBuilder promptBuilder)
    : IAnswerGenerator
{
    protected virtual IPromptBuilder Builder { get; } = promptBuilder;
    protected virtual IKnowledgeRetriever Retriever { get; } = retriever;
    protected virtual ChatClient Client { get; } = chatClient;

    public async Task<string> GenerateAsync(string question, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        var knowledge = await Retriever.SearchAsync(question, top: 5, cancellationToken);

        var prompt = Builder.Build(question, knowledge);

        var completion = await Client.CompleteChatAsync(prompt, cancellationToken);

        return completion.Value.Content.First().Text;
    }
}
