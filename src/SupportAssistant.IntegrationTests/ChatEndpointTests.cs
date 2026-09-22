using System.Diagnostics.CodeAnalysis;

namespace SupportAssistant.IntegrationTests;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public sealed class ChatEndpointTests
{
    private static readonly HttpClient Client = new()
    {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("SUPPORT_ASSISTANT_API_BASE_URL") ?? "http://localhost:7071")
    };

    [Fact]
    public async Task Chat_WithInvalidBody_ReturnsBadRequest()
    {
        using var content = new StringContent("{ invalid json }", System.Text.Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/chat", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Chat_WithEmptyQuestion_ReturnsBadRequest()
    {
        using var content = new StringContent(
            """
            {
              "question": ""
            }
            """, System.Text.Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/chat", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Chat_WithTooLongQuestion_ReturnsBadRequest()
    {
        string question = new('a', 4001);

        var json =
            $$"""
              {
                "question": "{{question}}"
              }
              """;

        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/chat", content);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
