using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Azure.AI.TextAnalytics;
using Azure.Identity;
using Azure.Search.Documents;

namespace SupportAssistant.IntegrationTests;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public sealed class AzureServicesTests
{
    [Fact]
    public async Task Search_ShouldReturnKnowledgeDocuments()
    {
        var endpoint = new Uri("https://search-isa-dev-uxfdhanj.search.windows.net");

        var client = new SearchClient(endpoint, "knowledge-index", new DefaultAzureCredential());

        var response = await client.SearchAsync<SearchTestDocument>("warranty");

        var results = new List<SearchTestDocument>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            results.Add(result.Document);
        }

        Assert.NotEmpty(results);
        Assert.Contains(
            results,
            x => x.Content.Contains(
                "warranty",
                StringComparison.OrdinalIgnoreCase));
    }

    private sealed class SearchTestDocument
    {
        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; init; } = string.Empty;
    }

    [Fact]
    public async Task Language_ShouldDetectNegativeSentiment()
    {
        var endpoint = new Uri("https://ai-isa-dev-uxfdhanj.cognitiveservices.azure.com/");

        var client = new TextAnalyticsClient(endpoint, new DefaultAzureCredential());

        var response = await client.AnalyzeSentimentAsync("This service is absolutely terrible.");

        Assert.Equal(TextSentiment.Negative, response.Value.Sentiment);
    }

    [Fact]
    public async Task Embeddings_ShouldReturnVector()
    {
        var client = new AzureOpenAIClient(
            new Uri("https://ai-isa-dev-uxfdhanj.openai.azure.com/"),
            new DefaultAzureCredential());

        var embeddingClient = client.GetEmbeddingClient("text-embedding-3-small");

        var response = await embeddingClient.GenerateEmbeddingAsync("warranty");

        var vector = response.Value.ToFloats();

        Assert.NotEqual(0, vector.Length);
        Assert.Equal(1536, vector.Length);
    }
}
