using System.ClientModel;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using OpenAI.Chat;
using SupportAssistant.Api.Contracts.Chats;
using SupportAssistant.Api.Functions;
using SupportAssistant.Api.Mapping;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Knowledge;
using ChatFunction = SupportAssistant.Api.Functions.ChatFunction;

namespace SupportAssistant.UnitTests.Api;

public sealed class FunctionTests
{
    private static IMapper CreateMapper() => new MapperConfiguration(config => config.AddProfile<DtoProfile>(), NullLoggerFactory.Instance).CreateMapper();

    private static HttpRequest Request(string body)
    {
        var context = new DefaultHttpContext { TraceIdentifier = "test-request-42" };
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return context.Request;
    }

    [Theory]
    [InlineData("{broken", "Invalid JSON request body.")]
    [InlineData("", "Invalid JSON request body.")]
    [InlineData("null", "Request body is required.")]
    [InlineData("{}", "Question is required.")]
    [InlineData("{\"question\":null}", "Question is required.")]
    [InlineData("{\"question\":\"\"}", "Question is required.")]
    [InlineData("{\"question\":\"  \\t\"}", "Question is required.")]
    public async Task ChatRejectsInvalidBodiesBeforeCallingService(string body, string error)
    {
        var service = new StubChatService();
        var request = Request(body);
        using var stream = request.Body;
        var result = Assert.IsType<BadRequestObjectResult>(await new ChatFunction(service, CreateMapper(), NullLogger<ChatFunction>.Instance).RunAsync(request, default));
        Assert.Equal(error, JsonSerializer.SerializeToElement(result.Value).GetProperty("error").GetString());
        Assert.Empty(service.Calls);
    }

    [Theory]
    [InlineData(4000, true)]
    [InlineData(4001, false)]
    public async Task ChatEnforcesQuestionLengthBoundary(int length, bool accepted)
    {
        var service = new StubChatService();
        var question = new string('x', length);
        var request = Request(JsonSerializer.Serialize(new { question }));
        using var stream = request.Body;
        using var cancellation = new CancellationTokenSource();
        var result = await new ChatFunction(service, CreateMapper(), NullLogger<ChatFunction>.Instance).RunAsync(request, cancellation.Token);

        if (accepted)
        {
            var response = Assert.IsType<ChatResponse>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.Equal("Generated answer", response.Answer);
            Assert.Equal(new[] { "policy.pdf" }, response.Sources);
            Assert.True(response.EscalationRequired);
            Assert.Equal("test-request-42", response.RequestId);
            Assert.Equal((new ChatCommand(question), cancellation.Token), Assert.Single(service.Calls));
        }
        else
        {
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Question is too long.", JsonSerializer.SerializeToElement(badRequest.Value).GetProperty("error").GetString());
            Assert.Empty(service.Calls);
        }
    }

    [Theory]
    [InlineData("azure", 502, "An Azure service request failed.")]
    [InlineData("openai", 502, "The AI service request failed.")]
    [InlineData("cancel", 499, "The request was cancelled.")]
    [InlineData("unexpected", 500, "An unexpected error occurred.")]
    public async Task ChatMapsFailuresToSafeErrorsWithRequestId(string kind, int status, string message)
    {
        Exception failure = kind switch
        {
            "azure" => new RequestFailedException(503, "private details"),
            "openai" => new ClientResultException("private details"),
            "cancel" => new OperationCanceledException("private details"),
            _ => new InvalidOperationException("private details")
        };
        var request = Request("""{"question":"Help me"}""");
        using var stream = request.Body;
        var result = Assert.IsType<ObjectResult>(await new ChatFunction(new StubChatService { Failure = failure }, CreateMapper(), NullLogger<ChatFunction>.Instance).RunAsync(request, default));
        Assert.Equal(status, result.StatusCode);
        var body = JsonSerializer.SerializeToElement(result.Value);
        Assert.Equal(message, body.GetProperty("error").GetString());
        Assert.Equal("test-request-42", body.GetProperty("requestId").GetString());
        Assert.DoesNotContain("private details", body.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ChatHandlesBadHttpRequestWhileReadingBody()
    {
        var request = Request("");
        request.Body.Dispose();
        using var stream = new BrokenRequestStream();
        request.Body = stream;
        var service = new StubChatService();
        var result = Assert.IsType<BadRequestObjectResult>(await new ChatFunction(service, CreateMapper(), NullLogger<ChatFunction>.Instance).RunAsync(request, default));
        Assert.Equal("Invalid request body.", JsonSerializer.SerializeToElement(result.Value).GetProperty("error").GetString());
        Assert.Empty(service.Calls);
    }

    [Fact]
    public void ChatConstructorRejectsMissingDependencies()
    {
        Assert.Throws<ArgumentNullException>("service", () => new ChatFunction(null!, CreateMapper(), NullLogger<ChatFunction>.Instance));
        Assert.Throws<ArgumentNullException>("mapper", () => new ChatFunction(new StubChatService(), null!, NullLogger<ChatFunction>.Instance));
    }

    [Theory]
    [InlineData("")]
    [InlineData("?q=")]
    [InlineData("?q=%20%20")]
    public async Task SearchRequiresQuery(string query)
    {
        var retriever = new StubRetriever();
        var request = new DefaultHttpContext().Request;
        request.QueryString = new QueryString(query);
        var result = Assert.IsType<BadRequestObjectResult>(await new SearchKnowledgeFunction(retriever).RunAsync(request, default));
        Assert.Equal("Query parameter 'q' is required.", JsonSerializer.SerializeToElement(result.Value).GetProperty("error").GetString());
        Assert.Empty(retriever.Calls);
    }

    [Fact]
    public async Task SearchForwardsDecodedQueryAndCancellationAndReturnsMatches()
    {
        var retriever = new StubRetriever();
        var request = new DefaultHttpContext().Request;
        request.QueryString = new QueryString("?q=return%20policy");
        using var cancellation = new CancellationTokenSource();
        var result = Assert.IsType<OkObjectResult>(await new SearchKnowledgeFunction(retriever).RunAsync(request, cancellation.Token));
        Assert.Same(retriever.Results, result.Value);
        Assert.Equal(("return policy", 5, cancellation.Token), Assert.Single(retriever.Calls));
    }

    private sealed class StubChatService : IChatService
    {
        public Exception? Failure { get; init; }
        public List<(ChatCommand, CancellationToken)> Calls { get; } = [];
        public Task<ChatResult> HandleAsync(ChatCommand command, CancellationToken cancellationToken = default)
        {
            Calls.Add((command, cancellationToken));
            return Failure is null ? Task.FromResult(new ChatResult("Generated answer", ["policy.pdf"], true)) : Task.FromException<ChatResult>(Failure);
        }
    }

    private sealed class StubRetriever : IKnowledgeRetriever
    {
        public IReadOnlyCollection<KnowledgeSearchResult> Results { get; } = [new("Policy", "policy.pdf", 2, 0.8)];
        public List<(string, int, CancellationToken)> Calls { get; } = [];
        public Task<IReadOnlyCollection<KnowledgeSearchResult>> SearchAsync(string query, int top = 5, CancellationToken cancellationToken = default)
        {
            Calls.Add((query, top, cancellationToken));
            return Task.FromResult(Results);
        }
    }

    private sealed class BrokenRequestStream : MemoryStream
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => ValueTask.FromException<int>(new BadHttpRequestException("broken body"));
    }
}
