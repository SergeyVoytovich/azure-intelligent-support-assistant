namespace SupportAssistant.Application.Knowledge;

public interface ITextChunker
{
    IReadOnlyCollection<TextChunk> Chunk(string text, string source, int? page = null);
}
