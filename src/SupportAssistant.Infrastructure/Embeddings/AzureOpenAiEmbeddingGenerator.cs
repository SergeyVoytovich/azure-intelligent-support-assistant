using OpenAI.Embeddings;
using SupportAssistant.Application.Embeddings;

namespace SupportAssistant.Infrastructure.Embeddings;

public class AzureOpenAiEmbeddingGenerator(EmbeddingClient client) : IEmbeddingGenerator
{
    protected virtual EmbeddingClient Client { get; } = client;


    public async Task<IReadOnlyList<float>> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        OpenAIEmbedding  embeddings = await Client.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return embeddings.ToFloats().ToArray();
    }
}
