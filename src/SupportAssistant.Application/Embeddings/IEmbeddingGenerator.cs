namespace SupportAssistant.Application.Embeddings;

public interface IEmbeddingGenerator
{
    Task<IReadOnlyList<float>> GenerateAsync(string text, CancellationToken cancellationToken = default);
}
