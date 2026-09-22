#pragma warning disable OPENAI001

using System.ClientModel.Primitives;
using Azure.AI.DocumentIntelligence;
using Azure.AI.OpenAI;
using Azure.AI.TextAnalytics;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Responses;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Documents;
using SupportAssistant.Application.Embeddings;
using SupportAssistant.Application.Knowledge;
using SupportAssistant.Application.Language;
using SupportAssistant.Infrastructure.Chats;
using SupportAssistant.Infrastructure.Documents;
using SupportAssistant.Infrastructure.Embeddings;
using SupportAssistant.Infrastructure.Ingestion;
using SupportAssistant.Infrastructure.Knowledge;
using SupportAssistant.Infrastructure.Language;
using SupportAssistant.Infrastructure.Search;

namespace SupportAssistant.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, InfrastructureConfiguration config)
        => services
            .AddDocumentIntelligenceClient(config)
            .AddSingleton<ITextAnalyzer, AzureTextAnalyzer>()
            .AddSingleton<IDocumentAnalyzer, AzureDocumentAnalyzer>()
            .AddScoped<IChatService, ChatService>()
            .AddAnswerGenerator(config)
            .AddSingleton<ITextChunker>(TextChunker.Default())
            .AddSearchIndexClient(config)
            .AddSingleton<SearchIndexInitializer>()
            .AddSingleton(new SearchClient(new Uri(config.SearchEndpoint), SearchIndexDefinition.IndexName,
                new DefaultAzureCredential()))
            .AddOpenAi(config)
            .AddSingleton<IEmbeddingGenerator, AzureOpenAiEmbeddingGenerator>()
            .AddBlobServiceClient(config)
            .AddSingleton<KnowledgeIngestionService>()
            .AddSingleton<SearchDocumentIndexer>()
            .AddSingleton<IKnowledgeRetriever, AzureKnowledgeRetriever>()
            .AddTextAnalyticsSlient(config)
        ;

    private static IServiceCollection AddAnswerGenerator(this IServiceCollection services, InfrastructureConfiguration config)
        => services.AddScoped<IAnswerGenerator>(p => new AzureOpenAiAnswerGenerator(
                p.GetRequiredService<ResponsesClient>(),
                p.GetRequiredService<IKnowledgeRetriever>(),
                p.GetRequiredService<IPromptBuilder>(),
                config.FoundryChatDeployment
            ))
        ;

    private static IServiceCollection AddDocumentIntelligenceClient(this IServiceCollection services, InfrastructureConfiguration config)
        => services.AddSingleton(
                new DocumentIntelligenceClient(
                    GetRequiredUri(config.DocumentsEndpoint, nameof(config.DocumentsEndpoint)),
                    new DefaultAzureCredential()
                )
            );

    private static IServiceCollection AddBlobServiceClient(this IServiceCollection services, InfrastructureConfiguration config)
        => services.AddSingleton(
            new BlobServiceClient(
                    GetRequiredUri(config.BlobServiceEndpoint, nameof(config.BlobServiceEndpoint)),
                    new DefaultAzureCredential()).GetBlobContainerClient(config.ContainerName)
        );

    private static IServiceCollection AddSearchIndexClient(this IServiceCollection services, InfrastructureConfiguration config)
        => services.AddSingleton(
            new SearchIndexClient(
                GetRequiredUri(config.SearchEndpoint, nameof(config.SearchEndpoint)),
                new DefaultAzureCredential())
        );

    private static IServiceCollection AddOpenAi(this IServiceCollection services, InfrastructureConfiguration config)
    {
        var openAiClient = new AzureOpenAIClient(
            GetRequiredUri(config.FoundryEndpoint, nameof(config.FoundryEndpoint)),
            new DefaultAzureCredential());
        return services
                .AddSingleton(openAiClient.GetEmbeddingClient(config.EmbeddingDeployment))
                .AddResponseClient(config)
            ;
    }

    private static IServiceCollection AddResponseClient(this IServiceCollection services, InfrastructureConfiguration config)
        => services.AddSingleton<ResponsesClient>(_ =>
        {
            var tokenPolicy = new BearerTokenPolicy(new DefaultAzureCredential(), "https://ai.azure.com/.default");

            return new ResponsesClient(
                authenticationPolicy: tokenPolicy,
                options: new ResponsesClientOptions
                {
                    Endpoint = GetRequiredUri(config.FoundryResponsesEndpoint, nameof(config.FoundryResponsesEndpoint))
                });
        });

    private static Uri GetRequiredUri(string? value, string name)
        => !Uri.TryCreate(value, UriKind.Absolute, out var uri)
            ? throw new InvalidOperationException($"Configuration '{name}' contains invalid URI: '{value ?? "<null>"}'")
            : uri;

    private static IServiceCollection AddTextAnalyticsSlient(this IServiceCollection services, InfrastructureConfiguration config)
        => services.AddSingleton(
                new TextAnalyticsClient(
                    GetRequiredUri(config.LanguageEndpoint, nameof(config.LanguageEndpoint)),
                    new DefaultAzureCredential()))
            .AddSingleton<ITextAnalyzer, AzureTextAnalyzer>();
}
