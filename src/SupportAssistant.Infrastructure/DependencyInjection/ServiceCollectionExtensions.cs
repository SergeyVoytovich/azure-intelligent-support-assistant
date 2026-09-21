using Azure.AI.DocumentIntelligence;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Documents;
using SupportAssistant.Application.Embeddings;
using SupportAssistant.Application.Knowledge;
using SupportAssistant.Infrastructure.Chats;
using SupportAssistant.Infrastructure.Documents;
using SupportAssistant.Infrastructure.Embeddings;
using SupportAssistant.Infrastructure.Ingestion;
using SupportAssistant.Infrastructure.Knowledge;
using SupportAssistant.Infrastructure.Search;

namespace SupportAssistant.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, InfrastrubtireConfiguration config)
        => services
            .AddSingleton(new DocumentIntelligenceClient(new Uri(config.DocumentsEndpoint), new DefaultAzureCredential()))
            .AddSingleton<IDocumentAnalyzer, AzureDocumentAnalyzer>()
            .AddScoped<IChatService, ChatService>()
            .AddScoped<IAnswerGenerator, AzureOpenAiAnswerGenerator>()
            .AddSingleton<ITextChunker>(TextChunker.Default())
            .AddSingleton(new SearchIndexClient(new Uri(config.SearchEndpoint), new DefaultAzureCredential()))
            .AddSingleton<SearchIndexInitializer>()
            .AddSingleton(new SearchClient(new Uri(config.SearchEndpoint), SearchIndexDefinition.IndexName,  new DefaultAzureCredential()))
            .AddOpenAi(config)
            .AddSingleton<IEmbeddingGenerator, AzureOpenAiEmbeddingGenerator>()
            .AddSingleton(new BlobServiceClient(new Uri(config.BoobServiceEndpoint), new DefaultAzureCredential())
                                .GetBlobContainerClient(config.ContainerName))
            .AddSingleton<KnowledgeIngestionService>()
            .AddSingleton<SearchDocumentIndexer>()
            .AddSingleton<IKnowledgeRetriever, AzureKnowledgeRetriever>()

        ;

    private static IServiceCollection AddOpenAi(this IServiceCollection services, InfrastrubtireConfiguration config)
    {
        var openAiClient = new AzureOpenAIClient(new Uri(config.FoundryEndpoint), new DefaultAzureCredential());
        return services
                .AddSingleton(openAiClient.GetEmbeddingClient(config.EmbeddingDeployment))
                .AddSingleton(openAiClient.GetChatClient(config.FoundryChatDeployment))
            ;
    }
}


