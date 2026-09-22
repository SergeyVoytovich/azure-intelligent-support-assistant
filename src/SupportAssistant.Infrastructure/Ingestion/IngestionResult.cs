namespace SupportAssistant.Infrastructure.Ingestion;

public sealed record IngestionResult(
    int BlobCount,
    int PdfCount,
    int ChunkCount,
    IReadOnlyCollection<string> BlobNames);
