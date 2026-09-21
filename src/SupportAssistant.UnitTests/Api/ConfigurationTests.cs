using Microsoft.Extensions.Configuration;
using SupportAssistant.Api.Configuration;

namespace SupportAssistant.UnitTests.Api;

public sealed class ConfigurationTests
{
    private static Dictionary<string, string?> Settings() => new()
    {
        ["AzureSearch:Endpoint"] = "https://search.test/",
        ["DocumentIntelligence:Endpoint"] = "https://documents.test/",
        ["Foundry:EmbeddingDeployment"] = "embedding",
        ["Foundry:Endpoint"] = "https://foundry.test/",
        ["Foundry:ChatDeployment"] = "chat",
        ["Storage:KnowledgeContainer"] = "knowledge",
        ["Storage:AccountName"] = "storage",
        ["Foundry:ResponsesEndpoint"] = "https://responses.test/",
        ["Language:Endpoint"] = "https://language.test/"
    };

    [Fact]
    public void InfrastructureConfigurationReadsEverySetting()
    {
        using var config = new ConfigurationManager();
        config.AddInMemoryCollection(Settings());
        var result = config.GetInfrastructureConfiguration();
        Assert.Equal("https://search.test/", result.SearchEndpoint);
        Assert.Equal("https://documents.test/", result.DocumentsEndpoint);
        Assert.Equal("embedding", result.EmbeddingDeployment);
        Assert.Equal("https://foundry.test/", result.FoundryEndpoint);
        Assert.Equal("chat", result.FoundryChatDeployment);
        Assert.Equal("knowledge", result.ContainerName);
        Assert.Equal("storage", result.StorageAccountName);
        Assert.Equal("https://responses.test/", result.FoundryResponsesEndpoint);
        Assert.Equal("https://language.test/", result.LanguageEndpoint);
    }

    [Theory]
    [InlineData("AzureSearch:Endpoint")]
    [InlineData("DocumentIntelligence:Endpoint")]
    [InlineData("Foundry:EmbeddingDeployment")]
    [InlineData("Foundry:Endpoint")]
    [InlineData("Foundry:ChatDeployment")]
    [InlineData("Storage:KnowledgeContainer")]
    [InlineData("Storage:AccountName")]
    [InlineData("Foundry:ResponsesEndpoint")]
    [InlineData("Language:Endpoint")]
    public void MissingSettingNamesTheRequiredKey(string key)
    {
        var values = Settings();
        values.Remove(key);
        using var config = new ConfigurationManager();
        config.AddInMemoryCollection(values);
        Assert.Equal($"'{key}' is not configured.", Assert.Throws<InvalidOperationException>(() => config.GetInfrastructureConfiguration()).Message);
    }
}
