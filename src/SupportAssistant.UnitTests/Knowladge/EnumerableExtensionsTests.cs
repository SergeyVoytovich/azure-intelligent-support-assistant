using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.UnitTests.Knowladge;

public sealed class EnumerableExtensionsTests
{
    [Theory]
    [InlineData("first", null, "middle")]
    [InlineData("middle", "first", "last")]
    [InlineData("last", "middle", null)]
    public void NeighborsRespectListBoundaries(string current, string? previous, string? next)
    {
        IList<string> values = new[] { "first", "middle", "last" };
        Assert.Equal(previous, values.Preview(current));
        Assert.Equal(next, values.Next(current));
    }

    [Fact]
    public void EmptyListHasNoNeighbors()
    {
        IList<string> values = Array.Empty<string>();
        Assert.Null(values.Preview("missing"));
        Assert.Null(values.Next("missing"));
    }

    [Fact]
    public void AppendRangePreservesOrderAndDoesNotMutateInputs()
    {
        int[] source = [1, 2];
        int[] appended = [3, 4];
        Assert.Equal(new[] { 1, 2, 3, 4 }, source.Append(appended));
        Assert.Equal(new[] { 1, 2 }, source);
        Assert.Equal(new[] { 3, 4 }, appended);
        Assert.Equal(source, source.Append(Array.Empty<int>()));
    }

    [Fact]
    public void PrependAppliesEachItemToFrontWithoutMutatingInputs()
    {
        int[] source = [1, 2];
        int[] prepended = [3, 4];
        Assert.Equal(new[] { 4, 3, 1, 2 }, source.Prepend(prepended));
        Assert.Equal(new[] { 1, 2 }, source);
        Assert.Equal(new[] { 3, 4 }, prepended);
        Assert.Equal(source, source.Prepend(Array.Empty<int>()));
    }
}
