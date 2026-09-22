using SupportAssistant.Application.Embeddings;

namespace SupportAssistant.UnitTests.TestDoubles;

internal sealed class StubEmbeddingGenerator : IEmbeddingGenerator
{
    public List<(string Text, CancellationToken Token)> Calls { get; } = [];
    public IReadOnlyList<float> Vector { get; init; } = [0.25f, -0.5f, 0.75f];
    public Exception? Failure { get; init; }

    public Task<IReadOnlyList<float>> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        Calls.Add((text, cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();
        return Failure is null ? Task.FromResult(Vector) : Task.FromException<IReadOnlyList<float>>(Failure);
    }
}
