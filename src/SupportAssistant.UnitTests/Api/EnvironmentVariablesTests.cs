using SupportAssistant.Api.System;

namespace SupportAssistant.UnitTests.Api;

// Environment variables are process-wide, so these tests must not overlap other tests.
[CollectionDefinition("Process environment", DisableParallelization = true)]
public sealed class ProcessEnvironmentDefinition;

[Collection("Process environment")]
public sealed class EnvironmentVariablesTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    [InlineData("InstrumentationKey=test", true)]
    public void TelemetryIsEnabledOnlyForNonWhitespaceConnectionString(string? value, bool expected)
    {
        const string name = "APPLICATIONINSIGHTS_CONNECTION_STRING";
        var original = Environment.GetEnvironmentVariable(name);
        try
        {
            Environment.SetEnvironmentVariable(name, value);
            Assert.Equal(value, EnvironmentVariables.ApplicationinsightsConnectionString);
            Assert.Equal(expected, EnvironmentVariables.IsApplicationinsightsConnected);
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, original);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("https://documents.test/")]
    public void RequiredDocumentEndpointIsReadOrNamesMissingVariable(string? value)
    {
        const string name = "DOCUMENT_INTELLIGENCE_ENDPOINT";
        var original = Environment.GetEnvironmentVariable(name);
        try
        {
            Environment.SetEnvironmentVariable(name, value);
            if (value is null)
            {
                var error = Assert.Throws<InvalidOperationException>(() => EnvironmentVariables.GetDocumentintelligenceendpoint);
                Assert.Equal("'DOCUMENT_INTELLIGENCE_ENDPOINT' is not configured.", error.Message);
            }
            else
            {
                Assert.Equal(value, EnvironmentVariables.GetDocumentintelligenceendpoint);
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, original);
        }
    }
}
