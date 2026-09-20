using Microsoft.Extensions.Configuration;

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

}
