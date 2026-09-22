using System.Diagnostics.CodeAnalysis;
using SupportAssistant.Infrastructure.DependencyInjection;

namespace SupportAssistant.UnitTests.DependencyInjection;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public sealed class InfrastructureConfigurationTests
{
    private static InfrastructureConfiguration CreateConfiguration()
        => new()
        {
            DocumentsEndpoint = "https://documents.test/",
            SearchEndpoint = "https://search.test/",
            FoundryEndpoint = "https://foundry.test/",
            FoundryChatDeployment = "gpt-4o",
            FoundryResponsesEndpoint = "https://responses.test/",
            EmbeddingDeployment = "text-embedding-3-small",
            StorageAccountName = "storagetest",
            ContainerName = "knowledge",
            LanguageEndpoint = "https://language.test/"
        };

    [Fact]
    public void BlobServiceEndpoint_ShouldBeBuiltFromStorageAccountName()
    {
        var configuration = CreateConfiguration();

        Assert.Equal("https://storagetest.blob.core.windows.net", configuration.BlobServiceEndpoint);
    }

    [Fact]
    public void Configuration_ShouldStoreProvidedValues()
    {
        var configuration = CreateConfiguration();

        Assert.Equal("https://documents.test/", configuration.DocumentsEndpoint);
        Assert.Equal("https://search.test/", configuration.SearchEndpoint);
        Assert.Equal("https://foundry.test/", configuration.FoundryEndpoint);
        Assert.Equal("gpt-4o", configuration.FoundryChatDeployment);
        Assert.Equal("https://responses.test/", configuration.FoundryResponsesEndpoint);
        Assert.Equal("text-embedding-3-small", configuration.EmbeddingDeployment);
        Assert.Equal("storagetest", configuration.StorageAccountName);
        Assert.Equal("knowledge", configuration.ContainerName);
        Assert.Equal("https://language.test/", configuration.LanguageEndpoint);
    }

    [Fact]
    public void ToString_ShouldContainAllConfigurationValues()
    {
        var configuration = CreateConfiguration();

        var result = configuration.ToString();

        Assert.Contains(nameof(configuration.DocumentsEndpoint), result);
        Assert.Contains(configuration.DocumentsEndpoint, result);

        Assert.Contains(nameof(configuration.SearchEndpoint), result);
        Assert.Contains(configuration.SearchEndpoint, result);

        Assert.Contains(nameof(configuration.FoundryEndpoint), result);
        Assert.Contains(configuration.FoundryEndpoint, result);

        Assert.Contains(nameof(configuration.FoundryChatDeployment), result);
        Assert.Contains(configuration.FoundryChatDeployment, result);

        Assert.Contains(nameof(configuration.FoundryResponsesEndpoint), result);
        Assert.Contains(configuration.FoundryResponsesEndpoint, result);

        Assert.Contains(nameof(configuration.EmbeddingDeployment), result);
        Assert.Contains(configuration.EmbeddingDeployment, result);

        Assert.Contains(nameof(configuration.StorageAccountName), result);
        Assert.Contains(configuration.StorageAccountName, result);

        Assert.Contains(nameof(configuration.ContainerName), result);
        Assert.Contains(configuration.ContainerName, result);

        Assert.Contains(nameof(configuration.BlobServiceEndpoint), result);
        Assert.Contains(configuration.BlobServiceEndpoint, result);

        Assert.Contains(nameof(configuration.LanguageEndpoint), result);
        Assert.Contains(configuration.LanguageEndpoint, result);
    }
}
