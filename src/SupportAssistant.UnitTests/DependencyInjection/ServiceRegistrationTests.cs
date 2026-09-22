#pragma warning disable OPENAI001
using Azure.AI.DocumentIntelligence;
using Azure.AI.TextAnalytics;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Embeddings;
using OpenAI.Responses;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.DependencyInjection;
using SupportAssistant.Application.Documents;
using SupportAssistant.Application.Embeddings;
using SupportAssistant.Application.Escalation;
using SupportAssistant.Application.Knowledge;
using SupportAssistant.Application.Language;
using SupportAssistant.Infrastructure.Chats;
using SupportAssistant.Infrastructure.DependencyInjection;
using SupportAssistant.Infrastructure.Documents;
using SupportAssistant.Infrastructure.Embeddings;
using SupportAssistant.Infrastructure.Ingestion;
using SupportAssistant.Infrastructure.Knowledge;
using SupportAssistant.Infrastructure.Language;
using SupportAssistant.Infrastructure.Search;

namespace SupportAssistant.UnitTests.DependencyInjection;

public sealed class ServiceRegistrationTests
{
    private static InfrastructureConfiguration Configuration() => new()
    {
        DocumentsEndpoint = "https://documents.test/", SearchEndpoint = "https://search.test/",
        FoundryEndpoint = "https://foundry.test/", FoundryChatDeployment = "chat-model",
        FoundryResponsesEndpoint = "https://responses.test/", EmbeddingDeployment = "embedding-model",
        StorageAccountName = "storage", ContainerName = "knowledge", LanguageEndpoint = "https://language.test/"
    };

    [Fact]
    public void RegistrationsResolveEntireDependencyGraphWithExpectedLifetimes()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        Assert.Same(services, services.AddApplication());
        Assert.Same(services, services.AddInfrastructure(Configuration()));
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        Assert.IsType<PromptBuilder>(provider.GetRequiredService<IPromptBuilder>());
        Assert.IsType<EscalationPolicy>(provider.GetRequiredService<IEscalationPolicy>());
        Assert.IsType<AzureTextAnalyzer>(provider.GetRequiredService<ITextAnalyzer>());
        Assert.IsType<AzureDocumentAnalyzer>(provider.GetRequiredService<IDocumentAnalyzer>());
        Assert.IsType<TextChunker>(provider.GetRequiredService<ITextChunker>());
        Assert.IsType<AzureOpenAiEmbeddingGenerator>(provider.GetRequiredService<IEmbeddingGenerator>());
        Assert.IsType<AzureKnowledgeRetriever>(provider.GetRequiredService<IKnowledgeRetriever>());
        Assert.NotNull(provider.GetRequiredService<KnowledgeIngestionService>());
        Assert.NotNull(provider.GetRequiredService<SearchDocumentIndexer>());
        Assert.NotNull(provider.GetRequiredService<SearchIndexInitializer>());
        Assert.NotNull(provider.GetRequiredService<DocumentIntelligenceClient>());
        Assert.NotNull(provider.GetRequiredService<TextAnalyticsClient>());
        Assert.NotNull(provider.GetRequiredService<SearchClient>());
        Assert.NotNull(provider.GetRequiredService<SearchIndexClient>());
        Assert.NotNull(provider.GetRequiredService<EmbeddingClient>());
        Assert.NotNull(provider.GetRequiredService<ResponsesClient>());
        Assert.Equal(new Uri("https://storage.blob.core.windows.net/knowledge"), provider.GetRequiredService<BlobContainerClient>().Uri);

        var chat = first.ServiceProvider.GetRequiredService<IChatService>();
        Assert.IsType<ChatService>(chat);
        Assert.Same(chat, first.ServiceProvider.GetRequiredService<IChatService>());
        Assert.NotSame(chat, second.ServiceProvider.GetRequiredService<IChatService>());
        var generator = Assert.IsType<AzureOpenAiAnswerGenerator>(first.ServiceProvider.GetRequiredService<IAnswerGenerator>());
        Assert.Equal("chat-model", generator.Endpoint);
        Assert.Same(generator, first.ServiceProvider.GetRequiredService<IAnswerGenerator>());
        Assert.NotSame(generator, second.ServiceProvider.GetRequiredService<IAnswerGenerator>());
        Assert.Same(first.ServiceProvider.GetRequiredService<IKnowledgeRetriever>(), second.ServiceProvider.GetRequiredService<IKnowledgeRetriever>());
        Assert.Same(first.ServiceProvider.GetRequiredService<IPromptBuilder>(), second.ServiceProvider.GetRequiredService<IPromptBuilder>());
    }

    [Theory]
    [InlineData("DocumentsEndpoint")]
    [InlineData("SearchEndpoint")]
    [InlineData("FoundryEndpoint")]
    [InlineData("LanguageEndpoint")]
    [InlineData("FoundryResponsesEndpoint")]
    public void InvalidEndpointsIdentifyConfigurationProperty(string property)
    {
        var valid = Configuration();
        var config = property switch
        {
            "DocumentsEndpoint" => valid with { DocumentsEndpoint = "relative" },
            "SearchEndpoint" => valid with { SearchEndpoint = "relative" },
            "FoundryEndpoint" => valid with { FoundryEndpoint = "relative" },
            "LanguageEndpoint" => valid with { LanguageEndpoint = "relative" },
            _ => valid with { FoundryResponsesEndpoint = "relative" }
        };
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var services = new ServiceCollection().AddInfrastructure(config);
            using var provider = services.BuildServiceProvider();
            provider.GetRequiredService<ResponsesClient>();
        });
        Assert.Contains(property, exception.Message, StringComparison.Ordinal);
        Assert.Contains("relative", exception.Message, StringComparison.Ordinal);
    }
}
