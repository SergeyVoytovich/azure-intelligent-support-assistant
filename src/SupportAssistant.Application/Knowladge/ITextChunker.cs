namespace SupportAssistant.Application.Knowladge;

public interface ITextChunker
{
    IReadOnlyCollection<TextChunk> Chunk(string text, string source, int? page = null);
}
