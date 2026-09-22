using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.Infrastructure.Knowledge;

public sealed class TextChunker : ITextChunker
{
    private const int DefaultChunkSizeInWords = 600;
    private const int DefaultOverlapInWords = 90;

    private int Size { get; }
    private int Overlaps { get; set; }

    private TextChunker(int size, int overlaps)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(size, 0);
        if (overlaps < 0 || overlaps >= size)
        {
            throw new ArgumentOutOfRangeException(nameof(overlaps));
        }

        Size = size;
        Overlaps = overlaps;
    }

    #region ITextChunker

    public IReadOnlyCollection<TextChunk> Chunk(string text, string source, int? page = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return words is [] ? [] : Chunk(words, source, page).ToList();
    }

    private IEnumerable<TextChunk> Chunk(string[] words, string source, int? page = null)
    {
        var index = 0;
        var maxIndex = words.Length - 1 / (Size - Overlaps);
        var toProcess = words.ToList();
        while (toProcess.Count != 0 && index <= maxIndex)
        {
            yield return new TextChunk
            {
                Content = string.Join(' ', toProcess.Take(Size)),
                Index = index++,
                Source = source,
                Page = page
            };

            toProcess = [.. toProcess.Skip(Size - Overlaps)];
        }
    }

    #endregion

    public static TextChunker Default() => new(DefaultChunkSizeInWords, DefaultOverlapInWords);

    public TextChunker WithSize(int value) => new(value, Math.Min(Overlaps, value - 1));
    public TextChunker WithOverlap(int value) => new(Size, value);

    public static TextChunker ChunkSizeInWords(int value) => Default().WithSize(value);
    public static TextChunker OverlapInWords(int value) => Default().WithOverlap(value);
}
