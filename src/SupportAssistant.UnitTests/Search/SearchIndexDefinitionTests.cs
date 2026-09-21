using System.Diagnostics.CodeAnalysis;
using Azure.Search.Documents.Indexes.Models;
using SupportAssistant.Infrastructure.Search;

namespace SupportAssistant.UnitTests.Search;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public sealed class SearchIndexDefinitionTests
{
    [Fact]
    public void Fields_ShouldContainExpectedNumberOfFields()
    {
        var fields = SearchIndexDefinition.Fields;

        Assert.Equal(6, fields.Count);
    }

    [Fact]
    public void Fields_ShouldConfigureIdField()
    {
        var field = SearchIndexDefinition.Fields.Single(x => x.Name == SearchIndexDefinition.IdField);

        Assert.Equal(SearchFieldDataType.String, field.Type);
        Assert.True(field.IsKey);
        Assert.True(field.IsFilterable);
    }

    [Fact]
    public void Fields_ShouldConfigureContentField()
    {
        var field = SearchIndexDefinition.Fields.Single(x => x.Name == SearchIndexDefinition.ContentField);

        Assert.Equal(SearchFieldDataType.String, field.Type);
        Assert.True(field.IsSearchable);
    }

    [Fact]
    public void Fields_ShouldConfigureVectorField()
    {
        var field = SearchIndexDefinition.Fields.Single(x => x.Name == SearchIndexDefinition.ContentVectorField);

        Assert.Equal(SearchFieldDataType.Collection(SearchFieldDataType.Single), field.Type);

        Assert.True(field.IsSearchable);

        Assert.Equal(SearchIndexDefinition.EmbeddingDimensions, field.VectorSearchDimensions);

        Assert.Equal("vector-profile", field.VectorSearchProfileName);
    }

    [Fact]
    public void Fields_ShouldConfigureSourceField()
    {
        var field = SearchIndexDefinition.Fields.Single(x => x.Name == SearchIndexDefinition.SourceField);

        Assert.Equal(SearchFieldDataType.String, field.Type);
        Assert.True(field.IsFilterable);
        Assert.True(field.IsFacetable);
    }

    [Fact]
    public void Fields_ShouldConfigurePageNumberField()
    {
        var field = SearchIndexDefinition.Fields.Single(x => x.Name == SearchIndexDefinition.PageNumberField);

        Assert.Equal(SearchFieldDataType.Int32, field.Type);
        Assert.True(field.IsFilterable);
    }

    [Fact]
    public void Fields_ShouldConfigureChunkIndexField()
    {
        var field = SearchIndexDefinition.Fields.Single(x => x.Name == SearchIndexDefinition.ChunkIndexField);

        Assert.Equal(SearchFieldDataType.Int32, field.Type);
        Assert.True(field.IsFilterable);
    }
}
