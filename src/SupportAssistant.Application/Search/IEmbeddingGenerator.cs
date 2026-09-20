namespace SupportAssistant.Application.Search;

public interface IEmbeddingGenerator
{
    Task<IReadOnlyList<float>> GenerateAsync(string text, CancellationToken cancellationToken = default);
}
