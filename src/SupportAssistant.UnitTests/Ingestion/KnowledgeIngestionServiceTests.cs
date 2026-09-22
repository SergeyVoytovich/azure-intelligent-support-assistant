using System.Text;
using Azure;
using Azure.Core.Pipeline;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using SupportAssistant.Api.Functions;
using SupportAssistant.Application.Documents;
using SupportAssistant.Infrastructure.Ingestion;
using SupportAssistant.Infrastructure.Knowledge;
using SupportAssistant.Infrastructure.Search;
using SupportAssistant.UnitTests.TestDoubles;

namespace SupportAssistant.UnitTests.Ingestion;

public sealed class KnowledgeIngestionServiceTests : IDisposable
{
    private readonly StubHttpHandler handler = new();
    private readonly HttpClient http;
    private readonly StubContainer container = new();
    private readonly StubAnalyzer analyzer = new();
    private readonly StubEmbeddingGenerator embeddings = new();
    private readonly KnowledgeIngestionService service;

    public KnowledgeIngestionServiceTests()
    {
        http = new HttpClient(handler);
        var options = new SearchClientOptions { Transport = new HttpClientTransport(http), Retry = { MaxRetries = 0 } };
        var search = new SearchClient(new Uri("https://search.test"), "knowledge-index", new AzureKeyCredential("test"), options);
        var indexes = new SearchIndexClient(new Uri("https://search.test"), new AzureKeyCredential("test"), options);
        var logger = NullLogger<SearchDocumentIndexer>.Instance;
        service = new KnowledgeIngestionService(container, analyzer, TextChunker.ChunkSizeInWords(2).WithOverlap(0),
            new SearchDocumentIndexer(search, embeddings, logger), new SearchIndexInitializer(indexes), logger);
    }

    [Fact]
    public async Task IngestionProcessesOnlyPdfsAcrossPagesAndDisposesStreams()
    {
        handler.Enqueue("""{"name":"knowledge-index","fields":[]}""");
        handler.Enqueue("""{"value":[{"key":"guide-0","status":true,"statusCode":201},{"key":"guide-1","status":true,"statusCode":201}]}""");
        container.Names = ["readme.txt", "Guide.PDF", "empty.pdf", "photo.png"];
        analyzer.Contents.Enqueue("one two three four");
        analyzer.Contents.Enqueue("");
        using var cancellation = new CancellationTokenSource();

        var result = await service.RunAsync(cancellation.Token);

        Assert.Equal(4, result.BlobCount);
        Assert.Equal(2, result.PdfCount);
        Assert.Equal(2, result.ChunkCount);
        Assert.Equal(container.Names, result.BlobNames);
        Assert.Equal(new[] { "Guide.PDF", "empty.pdf" }, container.OpenedNames);
        Assert.Equal(cancellation.Token, container.ListToken);
        Assert.All(container.ReadTokens, token => Assert.Equal(cancellation.Token, token));
        Assert.All(analyzer.Tokens, token => Assert.Equal(cancellation.Token, token));
        Assert.Equal(new[] { "one two", "three four" }, embeddings.Calls.Select(x => x.Text));
        Assert.All(container.Streams, stream => Assert.False(stream.CanRead));
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(HttpMethod.Put, handler.Requests[0].Method);
    }

    [Fact]
    public async Task EmptyContainerStillEnsuresIndexExistsAndReturnsZeroCounts()
    {
        handler.Enqueue("""{"name":"knowledge-index","fields":[]}""");
        var response = Assert.IsType<OkObjectResult>(await new IngestKnowledgeFunction(service).RunAsync(new DefaultHttpContext().Request, default));
        var result = Assert.IsType<IngestionResult>(response.Value);
        Assert.Equal(0, result.BlobCount);
        Assert.Equal(0, result.PdfCount);
        Assert.Equal(0, result.ChunkCount);
        Assert.Empty(result.BlobNames);
        Assert.Empty(analyzer.Tokens);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task AnalysisFailureDisposesDocumentAndPreventsUpload()
    {
        handler.Enqueue("""{"name":"knowledge-index","fields":[]}""");
        container.Names = ["broken.pdf"];
        var failure = new InvalidOperationException("unreadable PDF");
        analyzer.Failure = failure;
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunAsync()));
        Assert.False(Assert.Single(container.Streams).CanRead);
        Assert.Empty(embeddings.Calls);
        Assert.Single(handler.Requests);
    }

    private sealed class StubAnalyzer : IDocumentAnalyzer
    {
        public Queue<string> Contents { get; } = new();
        public List<CancellationToken> Tokens { get; } = [];
        public Exception? Failure { get; set; }
        public Task<DocumentAnalysis> AnalyzeAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            Assert.True(stream.CanRead);
            Tokens.Add(cancellationToken);
            return Failure is null
                ? Task.FromResult(new DocumentAnalysis { Content = Contents.Dequeue(), PageCount = 1 })
                : Task.FromException<DocumentAnalysis>(Failure);
        }
    }

    private sealed class StubContainer : BlobContainerClient
    {
        public string[] Names { get; set; } = [];
        public CancellationToken ListToken { get; private set; }
        public List<string> OpenedNames { get; } = [];
        public List<MemoryStream> Streams { get; } = [];
        public List<CancellationToken> ReadTokens { get; } = [];

        public override AsyncPageable<BlobItem> GetBlobsAsync(GetBlobsOptions? options = null, CancellationToken cancellationToken = default)
        {
            ListToken = cancellationToken;
            var pages = Names.Chunk(2).Select((names, index) => Page<BlobItem>.FromValues(
                names.Select(name => BlobsModelFactory.BlobItem(name: name)).ToArray(), index == 0 ? "next" : null, null!));
            return AsyncPageable<BlobItem>.FromPages(pages);
        }

        public override BlobClient GetBlobClient(string blobName)
        {
            OpenedNames.Add(blobName);
            return new StubBlob(this);
        }

        private sealed class StubBlob(StubContainer owner) : BlobClient
        {
            public override Task<Stream> OpenReadAsync(long position = 0, int? bufferSize = null, BlobRequestConditions? conditions = null, CancellationToken cancellationToken = default)
            {
                owner.ReadTokens.Add(cancellationToken);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes("PDF bytes"));
                owner.Streams.Add(stream);
                return Task.FromResult<Stream>(stream);
            }
        }
    }

    public void Dispose()
    {
        http.Dispose();
        foreach (var stream in container.Streams) stream.Dispose();
    }
}
