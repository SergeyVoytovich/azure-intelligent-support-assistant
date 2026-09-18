using Azure.AI.DocumentIntelligence;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Documents;
using SupportAssistant.Infrastructure.Chats;

namespace SupportAssistant.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string endpoint)
        => services
            .AddSingleton(new DocumentIntelligenceClient(new Uri(endpoint), new DefaultAzureCredential()))
            .AddSingleton<IDocumentAnalyzer, IDocumentAnalyzer>()
            .AddScoped<IChatService, ChatService>()
            .AddScoped<IAnswerGenerator, StubAnswerGenerator>()
        ;
}


