using System.Diagnostics.CodeAnalysis;
using SupportAssistant.Infrastructure.Knowledge;

namespace SupportAssistant.UnitTests.Knowladge;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public sealed class TextChunkerTests
{
    private static string GetText(int wordsCount)
        => wordsCount > 10
            ? throw new ArgumentOutOfRangeException(nameof(wordsCount))
                : "one two three four five six seven eight nine ten"
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Take(wordsCount)
                    .Aggregate((a, b) => a + " " + b);

    [Fact]
    public void Chunk_WhenTextFitsIntoSingleChunk_ReturnsSingleChunk()
    {
        var result = TextChunker
                        .ChunkSizeInWords(10)
                        .WithOverlap(2)
                        .Chunk(GetText(5), "shipping.pdf");

        var chunk = Assert.Single(result);

        Assert.Equal(0, chunk.Index);
        Assert.Equal("shipping.pdf", chunk.Source);
        Assert.Equal(GetText(5), chunk.Content);
    }

    [Fact]
    public void Chunk_WhenTextExceedsChunkSize_ReturnsMultipleChunks()
    {
        var text = GetText(9);

        var result = TextChunker
                        .ChunkSizeInWords(5)
                        .WithOverlap(2)
                        .Chunk(text, "shipping.pdf");

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Chunk_WhenMultipleChunksCreated_AddsOverlap()
    {
        var text = GetText(8);

        var result = TextChunker
                    .ChunkSizeInWords(5)
                    .WithOverlap(2)
                    .Chunk(text, "shipping.pdf")
                    .ToArray();

        Assert.Equal("one two three four five", result[0].Content);
        Assert.Equal("four five six seven eight", result[1].Content);
    }

    [Fact]
    public void Chunk_AssignsSequentialChunkIndexes()
    {
        var result = TextChunker
            .ChunkSizeInWords(4)
            .WithOverlap(1)
            .Chunk(GetText(9), "warranty.pdf")
            .ToArray();

        Assert.Equal(Enumerable.Range(0, result.Length), result.Select(x => x.Index));
    }

    [Fact]
    public void Chunk_PreservesSourceAndPageNumber()
    {
        var text = GetText(6);

        var result = TextChunker
                    .ChunkSizeInWords(5)
                    .WithOverlap(1)
                    .Chunk(text, "returns-refunds.pdf", 3);

        Assert.All(result, chunk =>
        {
            Assert.Equal($"returns-refunds.pdf", chunk.Source);
            Assert.Equal(3, chunk.Page);
        });
    }

    [Fact]
    public void Chunk_WhenTextIsEmpty_ReturnsEmptyCollection()
    {
        var result = TextChunker.Default().Chunk(string.Empty, "empty.pdf");
        Assert.Empty(result);
    }

    [Fact]
    public void Constructor_WhenOverlapIsGreaterThanOrEqualToChunkSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TextChunker.ChunkSizeInWords(10).WithOverlap(10));
    }
}
