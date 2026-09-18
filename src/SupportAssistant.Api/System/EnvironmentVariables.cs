using SupportAssistant.Infrastructure.DependencyInjection;

namespace SupportAssistant.Api.System;

public static class EnvironmentVariables
{
    private static string GetEnvironmentVariableRequired(string name)
        => Environment.GetEnvironmentVariable(name)
           ?? throw new InvalidOperationException($"'{name}' is not configured.");

    public static InfrastrubtireConfiguration InfrastrubtireConfiguration
        => new()
        {
            DocumentsEndpoint = GetEnvironmentVariableRequired("DOCUMENT_INTELLIGENCE_ENDPOINT"),
            SearchEndpoint = GetEnvironmentVariableRequired("AZURE_SEARCH_ENDPOINT")
        };
}
