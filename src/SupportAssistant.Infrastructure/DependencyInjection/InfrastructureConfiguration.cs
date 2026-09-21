namespace SupportAssistant.Infrastructure.DependencyInjection;

public record InfrastructureConfiguration
{
    public required string DocumentsEndpoint { get; init; }
    public required string SearchEndpoint { get; init; }
    public required string FoundryEndpoint { get; init; }
    public required string FoundryChatDeployment { get; init; }
    public required string FoundryResponsesEndpoint { get; init; }
    public required string EmbeddingDeployment { get; init; }
    public required string StorageAccountName { get; init; }
    public required string ContainerName { get; init; }
    public string BlobServiceEndpoint => $"https://{StorageAccountName}.blob.core.windows.net";
    public required string LanguageEndpoint { get; init; }


    public override string ToString()
        => $"{nameof(DocumentsEndpoint)}: {DocumentsEndpoint}\r\n" +
           $"{nameof(SearchEndpoint)}: {SearchEndpoint}\r\n" +
           $"{nameof(FoundryEndpoint)}: {FoundryEndpoint}\r\n" +
           $"{nameof(FoundryChatDeployment)}: {FoundryChatDeployment}\r\n" +
           $"{nameof(FoundryResponsesEndpoint)}: {EmbeddingDeployment}\r\n" +
           $"{nameof(EmbeddingDeployment)}: {EmbeddingDeployment}\r\n" +
           $"{nameof(StorageAccountName)}: {StorageAccountName}\r\n" +
           $"{nameof(ContainerName)}: {ContainerName}\r\n" +
           $"{nameof(BlobServiceEndpoint)}: {BlobServiceEndpoint}\r\n" +
           $"{nameof(LanguageEndpoint)}: {LanguageEndpoint}\r\n";
}
