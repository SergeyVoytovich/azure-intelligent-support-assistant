using Azure.AI.DocumentIntelligence;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Documents;
using SupportAssistant.Application.Knowledge;
using SupportAssistant.Infrastructure.Chats;
using SupportAssistant.Infrastructure.Knowledge;
using SupportAssistant.Infrastructure.Search;

namespace SupportAssistant.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, InfrastrubtireConfiguration configuration)
        => services
            .AddSingleton(new DocumentIntelligenceClient(new Uri(configuration.DocumentsEndpoint), new DefaultAzureCredential()))
            .AddSingleton<IDocumentAnalyzer, IDocumentAnalyzer>()
            .AddScoped<IChatService, ChatService>()
            .AddScoped<IAnswerGenerator, StubAnswerGenerator>()
            .AddSingleton<ITextChunker, TextChunker>()
            .AddSingleton(new SearchIndexClient(new Uri(configuration.SearchEndpoint), new DefaultAzureCredential()))
            .AddSingleton<SearchIndexInitializer>()
            .AddSingleton(new SearchClient(new Uri(configuration.SearchEndpoint), SearchIndexDefinition.IndexName,  new DefaultAzureCredential()))
        ;
}


