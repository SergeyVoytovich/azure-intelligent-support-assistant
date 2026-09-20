using Microsoft.Extensions.Configuration;

namespace SupportAssistant.Api.Configuration;

public static class ConfigurationManagerExtensions
{
    public static string GetAzureSearchEndpoint(this IConfigurationManager config)
        => config["AzureSearch:Endpoint"] ?? throw new InvalidOperationException("AzureSearch:Endpoint is not configured.");
}
