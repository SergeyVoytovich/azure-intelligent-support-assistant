using SupportAssistant.Infrastructure.DependencyInjection;

namespace SupportAssistant.Api.System;

public static class EnvironmentVariables
{
    private static string GetEnvironmentVariableRequired(string name)
        => Environment.GetEnvironmentVariable(name)
           ?? throw new InvalidOperationException($"'{name}' is not configured.");

    public static string? ApplicationinsightsConnectionString
        => Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

    public static bool IsApplicationinsightsConnected
        => !string.IsNullOrWhiteSpace(ApplicationinsightsConnectionString);

    public static string GetDocumentintelligenceendpoint
        => GetEnvironmentVariableRequired("DOCUMENT_INTELLIGENCE_ENDPOINT");

}
