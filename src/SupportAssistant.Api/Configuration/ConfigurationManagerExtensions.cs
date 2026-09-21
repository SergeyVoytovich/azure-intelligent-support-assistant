using Microsoft.Extensions.Configuration;
using SupportAssistant.Infrastructure.DependencyInjection;

namespace SupportAssistant.Api.Configuration;

public static class ConfigurationManagerExtensions
{
    public static string GetRequired(this IConfiguration config, string key)
        => config[key] ?? throw new InvalidOperationException($"'{key}' is not configured.");

    public static string GetAzureSearchEndpoint(this IConfigurationManager config)
        => config.GetRequired("AzureSearch:Endpoint");

    public static string GetFoundryEndpoint(this IConfigurationManager config)
        => config.GetRequired("Foundry:Endpoint");

    public static string GetEmbeddingDeplyment(this IConfigurationManager config)
        => config.GetRequired("Foundry:EmbeddingDeployment");

    public static string GetStorageAccountName(this IConfigurationManager config)
        => config.GetRequired("Storage:AccountName");

    public static string GetContainerName(this IConfigurationManager config)
        => config.GetRequired("Storage:KnowledgeContainer");

    public static string GetDocumentIntelligenceEndpoint(this ConfigurationManager config)
        => config.GetRequired("DocumentIntelligence:Endpoint");

    public static string FoundryChatDeployemnt(this IConfigurationManager config)
        => config.GetRequired("Foundry:ChatDeployment");

    public static InfrastrubtireConfiguration GetInfrastructureConfiguration(this ConfigurationManager config)
        => new()
        {
            SearchEndpoint = config.GetAzureSearchEndpoint(),
            DocumentsEndpoint = config.GetDocumentIntelligenceEndpoint(),
            EmbeddingDeployment = config.GetEmbeddingDeplyment(),
            FoundryEndpoint = config.GetFoundryEndpoint(),
            FoundryChatDeployment = config.GetFoundryEndpoint(),
            ContainerName = config.GetContainerName(),
            StorageAccountName = config.GetStorageAccountName()
        };
}
