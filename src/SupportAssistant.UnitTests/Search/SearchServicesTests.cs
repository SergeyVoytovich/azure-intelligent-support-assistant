using System.Net;
using System.Text.Json;
using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Logging.Abstractions;
using SupportAssistant.Application.Knowledge;
using SupportAssistant.Infrastructure.Search;
using SupportAssistant.UnitTests.TestDoubles;

namespace SupportAssistant.UnitTests.Search;

public sealed class SearchServicesTests : IDisposable
{
    private readonly StubHttpHandler handler = new();
    private readonly HttpClient http;
    private readonly SearchClient client;
    private readonly SearchIndexClient indexClient;

    public SearchServicesTests()
    {
        http = new HttpClient(handler);
        var options = new SearchClientOptions { Transport = new HttpClientTransport(http), Retry = { MaxRetries = 0 } };
        client = new SearchClient(new Uri("https://search.test"), "knowledge-index", new AzureKeyCredential("test"), options);
        indexClient = new SearchIndexClient(new Uri("https://search.test"), new AzureKeyCredential("test"), options);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(12)]
    public async Task SearchBuildsHybridQueryAndMapsResults(int top)
    {
        handler.Enqueue("""{"value":[{"@search.score":0.9,"content":"Warranty policy","source":"warranty.pdf","pageNumber":3},{"content":null,"source":null,"pageNumber":null}]}""");
        var embeddings = new StubEmbeddingGenerator();
        using var cancellation = new CancellationTokenSource();

        var results = await new AzureKnowledgeRetriever(client, embeddings).SearchAsync("warranty", top, cancellation.Token);

        Assert.Collection(results,
            item => Assert.Equal(new KnowledgeSearchResult("Warranty policy", "warranty.pdf", 3, 0.9), item),
            item => Assert.Equal(new KnowledgeSearchResult("", "", null, 0), item));
        Assert.Equal(("warranty", cancellation.Token), Assert.Single(embeddings.Calls));
        var request = Assert.Single(handler.Requests);
        using var json = JsonDocument.Parse(request.Body);
        var body = json.RootElement;
        Assert.Equal("warranty", body.GetProperty("search").GetString());
        Assert.Equal(top, body.GetProperty("top").GetInt32());
        Assert.Equal("semantic", body.GetProperty("queryType").GetString());
        Assert.Equal("semantic-config", body.GetProperty("semanticConfiguration").GetString());
        Assert.Equal("content,source,pageNumber", body.GetProperty("select").GetString());
        var vector = Assert.Single(body.GetProperty("vectorQueries").EnumerateArray());
        Assert.Equal(top, vector.GetProperty("k").GetInt32());
        Assert.Equal("contentVector", vector.GetProperty("fields").GetString());
        Assert.Equal(embeddings.Vector, vector.GetProperty("vector").EnumerateArray().Select(x => x.GetSingle()));
    }

    [Fact]
    public async Task SearchReturnsEmptyWhenNoMatchesExist()
    {
        handler.Enqueue("""{"value":[]}""");
        Assert.Empty(await new AzureKnowledgeRetriever(client, new StubEmbeddingGenerator()).SearchAsync("unknown"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public async Task SearchRejectsInvalidQueryBeforeCallingDependencies(string? query)
    {
        var embeddings = new StubEmbeddingGenerator();
        await Assert.ThrowsAnyAsync<ArgumentException>(() => new AzureKnowledgeRetriever(client, embeddings).SearchAsync(query!));
        Assert.Empty(embeddings.Calls);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task EmbeddingFailurePreventsSearch()
    {
        var failure = new InvalidOperationException("embedding unavailable");
        var embeddings = new StubEmbeddingGenerator { Failure = failure };
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => new AzureKnowledgeRetriever(client, embeddings).SearchAsync("query")));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task SearchPropagatesServiceFailure()
    {
        handler.Enqueue("""{"error":{"code":"ServiceUnavailable","message":"offline"}}""", HttpStatusCode.ServiceUnavailable);
        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => new AzureKnowledgeRetriever(client, new StubEmbeddingGenerator()).SearchAsync("query"));
        Assert.Equal(503, exception.Status);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task IndexUploadsEmbeddingsAndPreservesChunkMetadata()
    {
        handler.Enqueue("""{"value":[{"key":"return-policy-2","status":true,"statusCode":201},{"key":"manual-0","status":true,"statusCode":200}]}""");
        var embeddings = new StubEmbeddingGenerator();
        using var cancellation = new CancellationTokenSource();
        TextChunk[] chunks =
        [
            new() { Content = "Return within 30 days", Source = "folder/Return Policy.PDF", Index = 2, Page = 7 },
            new() { Content = "Keep receipt", Source = "Manual.pdf", Index = 0 }
        ];

        await new SearchDocumentIndexer(client, embeddings, NullLogger<SearchDocumentIndexer>.Instance).IndexAsync(chunks, cancellation.Token);

        Assert.Equal(chunks.Select(x => (x.Content, cancellation.Token)), embeddings.Calls);
        using var json = JsonDocument.Parse(Assert.Single(handler.Requests).Body);
        var documents = json.RootElement.GetProperty("value").EnumerateArray().ToArray();
        Assert.Equal(2, documents.Length);
        for (var i = 0; i < documents.Length; i++)
        {
            var document = documents[i];
            Assert.Equal("upload", document.GetProperty("@search.action").GetString());
            Assert.Equal(chunks[i].Content, document.GetProperty("content").GetString());
            Assert.Equal(chunks[i].Source, document.GetProperty("source").GetString());
            Assert.Equal(chunks[i].Index, document.GetProperty("chunkIndex").GetInt32());
            Assert.Equal(embeddings.Vector, document.GetProperty("contentVector").EnumerateArray().Select(x => x.GetSingle()));
        }
        Assert.Equal("return-policy-2", documents[0].GetProperty("id").GetString());
        Assert.Equal(7, documents[0].GetProperty("pageNumber").GetInt32());
        Assert.Equal("manual-0", documents[1].GetProperty("id").GetString());
        Assert.Equal(JsonValueKind.Null, documents[1].GetProperty("pageNumber").ValueKind);
    }

    [Fact]
    public async Task EmptyIndexBatchDoesNotCallDependencies()
    {
        var embeddings = new StubEmbeddingGenerator();
        await new SearchDocumentIndexer(client, embeddings, NullLogger<SearchDocumentIndexer>.Instance).IndexAsync([]);
        Assert.Empty(embeddings.Calls);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task CanceledIndexBatchDoesNotUpload()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new SearchDocumentIndexer(client, new StubEmbeddingGenerator(), NullLogger<SearchDocumentIndexer>.Instance)
            .IndexAsync([new TextChunk { Content = "text", Source = "a.pdf" }], cancellation.Token));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task InitializerCreatesMatchingVectorAndSemanticConfiguration()
    {
        handler.Enqueue("""{"name":"knowledge-index","fields":[]}""");
        await new SearchIndexInitializer(indexClient).EnsureCreatedAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Put, request.Method);
        using var json = JsonDocument.Parse(request.Body);
        var body = json.RootElement;
        Assert.Equal("knowledge-index", body.GetProperty("name").GetString());
        Assert.Equal(6, body.GetProperty("fields").GetArrayLength());
        var vector = body.GetProperty("vectorSearch");
        var profile = Assert.Single(vector.GetProperty("profiles").EnumerateArray());
        Assert.Equal("vector-profile", profile.GetProperty("name").GetString());
        Assert.Equal("hnsw-config", profile.GetProperty("algorithm").GetString());
        Assert.Equal("hnsw", Assert.Single(vector.GetProperty("algorithms").EnumerateArray()).GetProperty("kind").GetString());
        var semantic = body.GetProperty("semantic");
        Assert.Equal("semantic-config", semantic.GetProperty("defaultConfiguration").GetString());
        var configuration = Assert.Single(semantic.GetProperty("configurations").EnumerateArray());
        Assert.Equal("semantic-config", configuration.GetProperty("name").GetString());
        Assert.Equal("content", Assert.Single(configuration.GetProperty("prioritizedFields").GetProperty("prioritizedContentFields").EnumerateArray()).GetProperty("fieldName").GetString());
    }

    public void Dispose() => http.Dispose();
}
