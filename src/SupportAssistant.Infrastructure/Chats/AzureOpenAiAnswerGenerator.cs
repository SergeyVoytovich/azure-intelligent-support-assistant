using OpenAI.Responses;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Knowledge;
#pragma warning disable OPENAI001

namespace SupportAssistant.Infrastructure.Chats;

public class AzureOpenAiAnswerGenerator(
    ResponsesClient chatClient, IKnowledgeRetriever retriever, IPromptBuilder promptBuilder, string endpoint)
    : IAnswerGenerator
{
    protected virtual IPromptBuilder Builder { get; } = promptBuilder;
    public string Endpoint { get; } = endpoint;
    protected virtual IKnowledgeRetriever Retriever { get; } = retriever;
    protected virtual ResponsesClient   Client { get; } = chatClient;

    public async Task<AnswerGenerationResult> GenerateAsync(string question, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        var knowledge = await Retriever.SearchAsync(question, top: 5, cancellationToken);

        var prompt = Builder.Build(question, knowledge);

        var response =
            await Client.CreateResponseAsync(
                Endpoint,
                prompt,
                null,
                cancellationToken);

        return new AnswerGenerationResult
        (
            Answer: response.Value.GetOutputText(),
            Sources: knowledge.Select(x => x.Source).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
        );
    }
}
