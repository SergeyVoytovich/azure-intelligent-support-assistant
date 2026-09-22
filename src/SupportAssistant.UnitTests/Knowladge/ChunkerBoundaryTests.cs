using SupportAssistant.Infrastructure.Knowledge;

namespace SupportAssistant.UnitTests.Knowladge;

public sealed class ChunkerBoundaryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveSizeIsRejected(int size)
        => Assert.Throws<ArgumentOutOfRangeException>(() => TextChunker.ChunkSizeInWords(size));

    [Theory]
    [InlineData(-1)]
    [InlineData(600)]
    [InlineData(601)]
    public void InvalidOverlapIsRejected(int overlap)
        => Assert.Throws<ArgumentOutOfRangeException>(() => TextChunker.OverlapInWords(overlap));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public void SourceMustBeProvided(string? source)
        => Assert.ThrowsAny<ArgumentException>(() => TextChunker.Default().Chunk("text", source!));

    [Fact]
    public void NullTextIsRejected()
        => Assert.Throws<ArgumentNullException>("text", () => TextChunker.Default().Chunk(null!, "a.pdf"));

    [Fact]
    public void WhitespaceOnlyTextCreatesNoChunks()
        => Assert.Empty(TextChunker.Default().Chunk(" \t\r\n", "a.pdf"));

    [Fact]
    public void ZeroOverlapPreservesAllWordsExactlyOnceAndNormalizesWhitespace()
    {
        var chunks = TextChunker.ChunkSizeInWords(2).WithOverlap(0).Chunk(" one\ttwo\r\nthree  four\nfive ", "a.pdf", 4).ToArray();
        Assert.Equal(new[] { "one two", "three four", "five" }, chunks.Select(x => x.Content));
        Assert.Equal(new[] { 0, 1, 2 }, chunks.Select(x => x.Index));
        Assert.All(chunks, chunk => { Assert.Equal("a.pdf", chunk.Source); Assert.Equal(4, chunk.Page); });
    }

    [Fact]
    public void FluentConfigurationLeavesOriginalChunkerUnchanged()
    {
        var original = TextChunker.ChunkSizeInWords(4).WithOverlap(0);
        var smaller = original.WithSize(2);
        Assert.Single(original.Chunk("a b c d", "a.pdf"));
        Assert.Equal(2, smaller.Chunk("a b c d", "a.pdf").Count);
    }

    [Fact]
    public void SizeOneClampsOverlapAndTerminates()
        => Assert.Equal(new[] { "a", "b", "c" }, TextChunker.ChunkSizeInWords(1).Chunk("a b c", "a.pdf").Select(x => x.Content));
}
