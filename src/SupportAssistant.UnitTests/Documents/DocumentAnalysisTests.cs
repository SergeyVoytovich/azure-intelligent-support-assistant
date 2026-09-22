using Azure;
using Azure.AI.DocumentIntelligence;
using SupportAssistant.Infrastructure.Documents;

namespace SupportAssistant.UnitTests.Documents;

public sealed class DocumentAnalysisTests
{
    [Theory]
    [InlineData("Extracted text", 2)]
    [InlineData(null, 0)]
    public async Task AnalyzerUsesLayoutModelAndMapsContentAndPages(string? content, int pageCount)
    {
        var client = new StubDocumentClient(content, pageCount);
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        using var cancellation = new CancellationTokenSource();

        var result = await new AzureDocumentAnalyzer(client).AnalyzeAsync(stream, cancellation.Token);

        Assert.Equal(content ?? "", result.Content);
        Assert.Equal(pageCount, result.PageCount);
        Assert.Equal(WaitUntil.Completed, client.Wait);
        Assert.Equal("prebuilt-layout", client.Model);
        Assert.Equal(new byte[] { 1, 2, 3 }, client.Data!.ToArray());
        Assert.Equal(cancellation.Token, client.Token);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task NullDocumentIsRejectedBeforeCallingAzure()
    {
        var client = new StubDocumentClient("text", 1);
        await Assert.ThrowsAsync<ArgumentNullException>("stream", () => new AzureDocumentAnalyzer(client).AnalyzeAsync(null!));
        Assert.Null(client.Data);
    }

    [Fact]
    public async Task StreamExtensionForwardsBytesAndCancellation()
    {
        var client = new StubDocumentClient("text", 1);
        using var stream = new MemoryStream(new byte[] { 4, 5, 6 });
        using var cancellation = new CancellationTokenSource();
        var result = await client.AnalyzePrebuiltAsync(stream, cancellation.Token);
        Assert.Same(client.Result, result.Value);
        Assert.Equal(new byte[] { 4, 5, 6 }, client.Data!.ToArray());
        Assert.Equal("prebuilt-layout", client.Model);
        Assert.Equal(WaitUntil.Completed, client.Wait);
        Assert.Equal(cancellation.Token, client.Token);
    }

    [Fact]
    public async Task BinaryExtensionForwardsOriginalData()
    {
        var client = new StubDocumentClient("text", 1);
        var data = BinaryData.FromString("document");
        await client.AnalyzePrebuiltAsync(data);
        Assert.Same(data, client.Data);
    }

    private sealed class StubDocumentClient(string? content, int pages) : DocumentIntelligenceClient
    {
        public AnalyzeResult Result { get; } = DocumentIntelligenceModelFactory.AnalyzeResult(
            content: content, pages: Enumerable.Range(1, pages).Select(page => DocumentIntelligenceModelFactory.DocumentPage(pageNumber: page)));
        public WaitUntil Wait { get; private set; }
        public string? Model { get; private set; }
        public BinaryData? Data { get; private set; }
        public CancellationToken Token { get; private set; }

        public override Task<Operation<AnalyzeResult>> AnalyzeDocumentAsync(WaitUntil waitUntil, string modelId, BinaryData bytesSource, CancellationToken cancellationToken = default)
        {
            Wait = waitUntil;
            Model = modelId;
            Data = bytesSource;
            Token = cancellationToken;
            return Task.FromResult<Operation<AnalyzeResult>>(new CompletedAnalysis(Result));
        }
    }

    private sealed class CompletedAnalysis(AnalyzeResult result) : Operation<AnalyzeResult>
    {
        public override string Id => "analysis";
        public override AnalyzeResult Value => result;
        public override bool HasCompleted => true;
        public override bool HasValue => true;
        public override Response GetRawResponse() => null!;
        public override Response UpdateStatus(CancellationToken cancellationToken = default) => null!;
        public override ValueTask<Response> UpdateStatusAsync(CancellationToken cancellationToken = default) => ValueTask.FromResult<Response>(null!);
    }
}
