using Microsoft.Extensions.Configuration;
using SupportAssistant.Infrastructure.DependencyInjection;

namespace SupportAssistant.Api.Configuration;

public static class ConfigurationManagerExtensions
{
    public static string GetRequired(this IConfiguration config, string key)
        => config[key] ?? throw new InvalidOperationException($"'{key}' is not configured.");

    public static InfrastructureConfiguration GetInfrastructureConfiguration(this ConfigurationManager config)
        => new()
        {
            SearchEndpoint = config.GetRequired("AzureSearch:Endpoint"),
            DocumentsEndpoint = config.GetRequired("DocumentIntelligence:Endpoint"),
            EmbeddingDeployment = config.GetRequired("Foundry:EmbeddingDeployment"),
            FoundryEndpoint = config.GetRequired("Foundry:Endpoint"),
            FoundryChatDeployment = config.GetRequired("Foundry:ChatDeployment"),
            ContainerName = config.GetRequired("Storage:KnowledgeContainer"),
            StorageAccountName = config.GetRequired("Storage:AccountName"),
            FoundryResponsesEndpoint = config.GetRequired("Foundry:ResponsesEndpoint"),
            LanguageEndpoint = config.GetRequired("Language:Endpoint")
        };
}
