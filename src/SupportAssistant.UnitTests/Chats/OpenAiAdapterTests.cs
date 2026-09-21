#pragma warning disable OPENAI001
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using OpenAI.Responses;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Knowledge;
using SupportAssistant.Infrastructure.Chats;
using SupportAssistant.Infrastructure.Embeddings;
using SupportAssistant.UnitTests.TestDoubles;

namespace SupportAssistant.UnitTests.Chats;

public sealed class OpenAiAdapterTests : IDisposable
{
    private readonly StubHttpHandler handler = new();
    private readonly HttpClient http;
    private readonly ResponsesClient responses;
    private readonly EmbeddingClient embeddings;
    private readonly ChatClient chat;

    public OpenAiAdapterTests()
    {
        http = new HttpClient(handler);
        var transport = new HttpClientPipelineTransport(http);
        var options = new OpenAIClientOptions { Endpoint = new Uri("https://openai.test/v1"), Transport = transport, RetryPolicy = new ClientRetryPolicy(0) };
        var credential = new ApiKeyCredential("test-key");
        responses = new ResponsesClient(credential, new ResponsesClientOptions
        {
            Endpoint = new Uri("https://openai.test/v1"), Transport = transport, RetryPolicy = new ClientRetryPolicy(0)
        });
        embeddings = new EmbeddingClient("embedding-model", credential, options);
        chat = new ChatClient("chat-model", credential, options);
    }

    [Fact]
    public async Task AnswerUsesRetrievedContextAndDeduplicatesSourcesIgnoringCase()
    {
        handler.Enqueue("""{"id":"resp_test","object":"response","created_at":1700000000,"status":"completed","model":"chat-model","output":[{"id":"msg_test","type":"message","status":"completed","role":"assistant","content":[{"type":"output_text","text":"The warranty lasts two years.","annotations":[]}]}]}""");
        var retriever = new StubRetriever
        {
            Results = [new("Two years", "Warranty.pdf", 1, 0.9), new("Keep receipt", "WARRANTY.PDF", 2, 0.8), new("Contact support", "support.pdf", null, 0.7)]
        };
        using var cancellation = new CancellationTokenSource();
        var result = await new AzureOpenAiAnswerGenerator(responses, retriever, new PromptBuilder(), "chat-model").GenerateAsync("How long is the warranty?", cancellation.Token);

        Assert.Equal("The warranty lasts two years.", result.Answer);
        Assert.Equal(new[] { "Warranty.pdf", "support.pdf" }, result.Sources);
        Assert.Equal(("How long is the warranty?", 5, cancellation.Token), Assert.Single(retriever.Calls));
        using var request = JsonDocument.Parse(Assert.Single(handler.Requests).Body);
        Assert.Equal("chat-model", request.RootElement.GetProperty("model").GetString());
        var input = request.RootElement.GetProperty("input").ToString();
        Assert.Contains("Two years", input, StringComparison.Ordinal);
        Assert.Contains("Keep receipt", input, StringComparison.Ordinal);
        Assert.Contains("Contact support", input, StringComparison.Ordinal);
        Assert.Contains("How long is the warranty?", input, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnswerSupportsNoKnowledgeMatches()
    {
        handler.Enqueue("""{"id":"resp_empty","object":"response","created_at":1700000000,"status":"completed","model":"chat-model","output":[{"id":"msg_test","type":"message","status":"completed","role":"assistant","content":[{"type":"output_text","text":"I do not have enough information.","annotations":[]}]}]}""");
        var result = await new AzureOpenAiAnswerGenerator(responses, new StubRetriever(), new PromptBuilder(), "chat-model").GenerateAsync("unknown");
        Assert.Equal("I do not have enough information.", result.Answer);
        Assert.Empty(result.Sources);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public async Task InvalidAnswerQuestionIsRejectedBeforeRetrieval(string? question)
    {
        var retriever = new StubRetriever();
        await Assert.ThrowsAnyAsync<ArgumentException>(() => new AzureOpenAiAnswerGenerator(responses, retriever, new PromptBuilder(), "chat-model").GenerateAsync(question!));
        Assert.Empty(retriever.Calls);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task RetrievalFailurePreventsModelRequest()
    {
        var failure = new InvalidOperationException("search failed");
        var retriever = new StubRetriever { Failure = failure };
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => new AzureOpenAiAnswerGenerator(responses, retriever, new PromptBuilder(), "chat-model").GenerateAsync("question")));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ModelFailureIsPropagatedWithoutRetry()
    {
        handler.Enqueue("""{"error":{"message":"offline","type":"server_error","code":"server_error"}}""", HttpStatusCode.ServiceUnavailable);
        var error = await Assert.ThrowsAsync<ClientResultException>(() => new AzureOpenAiAnswerGenerator(responses, new StubRetriever(), new PromptBuilder(), "chat-model").GenerateAsync("question"));
        Assert.Equal(503, error.Status);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task EmbeddingDecodesVectorAndSendsOriginalText()
    {
        float[] vector = [1f, -0.5f, 0.25f];
        var encoded = Convert.ToBase64String(vector.SelectMany(BitConverter.GetBytes).ToArray());
        handler.Enqueue(JsonSerializer.Serialize(new { data = new[] { new { @object = "embedding", index = 0, embedding = encoded } }, model = "embedding-model", usage = new { prompt_tokens = 2, total_tokens = 2 } }));

        var result = await new AzureOpenAiEmbeddingGenerator(embeddings).GenerateAsync("original text");

        Assert.Equal(vector, result);
        using var request = JsonDocument.Parse(Assert.Single(handler.Requests).Body);
        Assert.Equal("embedding-model", request.RootElement.GetProperty("model").GetString());
        Assert.Contains("original text", request.RootElement.GetProperty("input").ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \n")]
    public async Task InvalidEmbeddingTextDoesNotCallModel(string? text)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => new AzureOpenAiEmbeddingGenerator(embeddings).GenerateAsync(text!));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ChatExtensionSendsOneUserMessage()
    {
        handler.Enqueue("""{"id":"chat_test","object":"chat.completion","created":1700000000,"model":"chat-model","choices":[{"index":0,"message":{"role":"assistant","content":"answer"},"finish_reason":"stop"}],"usage":{"prompt_tokens":1,"completion_tokens":1,"total_tokens":2}}""");
        var result = await chat.CompleteChatAsync("my prompt");
        Assert.Equal("answer", Assert.Single(result.Value.Content).Text);
        using var request = JsonDocument.Parse(Assert.Single(handler.Requests).Body);
        var message = Assert.Single(request.RootElement.GetProperty("messages").EnumerateArray());
        Assert.Equal("user", message.GetProperty("role").GetString());
        Assert.Contains("my prompt", message.GetProperty("content").ToString(), StringComparison.Ordinal);
    }

    private sealed class StubRetriever : IKnowledgeRetriever
    {
        public IReadOnlyCollection<KnowledgeSearchResult> Results { get; init; } = [];
        public Exception? Failure { get; init; }
        public List<(string, int, CancellationToken)> Calls { get; } = [];
        public Task<IReadOnlyCollection<KnowledgeSearchResult>> SearchAsync(string query, int top = 5, CancellationToken cancellationToken = default)
        {
            Calls.Add((query, top, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
            return Failure is null ? Task.FromResult(Results) : Task.FromException<IReadOnlyCollection<KnowledgeSearchResult>>(Failure);
        }
    }

    public void Dispose() => http.Dispose();
}
