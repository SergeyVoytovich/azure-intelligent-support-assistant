namespace SupportAssistant.Api.System;

public static class EnvironmentVariables
{
    // ReSharper disable InconsistentNaming
    private const string DOCUMENT_INTELLIGENCE_ENDPOINT = nameof(DOCUMENT_INTELLIGENCE_ENDPOINT);
    // ReSharper restore InconsistentNaming

    private static string GetEnvironmentVariableRequired(string name)
        => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException($"'{name}' is not configured.");

    public static string DocumentIntelligenceEndpoint => GetEnvironmentVariableRequired(DOCUMENT_INTELLIGENCE_ENDPOINT);
}
