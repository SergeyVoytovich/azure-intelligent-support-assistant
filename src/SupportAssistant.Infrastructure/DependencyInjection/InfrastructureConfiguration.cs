namespace SupportAssistant.Infrastructure.DependencyInjection;

public record InfrastructureConfiguration
{
    public string DocumentsEndpoint { get; set; } = null!;
    public string SearchEndpoint { get; set; } = null!;
    public string FoundryEndpoint { get; set; } = null!;
    public string FoundryChatDeployment { get; set; } = null!;
    public string FoundryResponsesEndpoint { get; set; } = null!;
    public string EmbeddingDeployment { get; set; } = null!;
    public string StorageAccountName { get; set; } = null!;
    public string ContainerName { get; set; } = null!;
    public string BlobServiceEndpoint => $"https://{StorageAccountName}.blob.core.windows.net";


    public override string ToString()
        => $"{nameof(DocumentsEndpoint)}: {DocumentsEndpoint}\r\n" +
           $"{nameof(SearchEndpoint)}: {SearchEndpoint}\r\n" +
           $"{nameof(FoundryEndpoint)}: {FoundryEndpoint}\r\n" +
           $"{nameof(FoundryChatDeployment)}: {FoundryChatDeployment}\r\n" +
           $"{nameof(FoundryResponsesEndpoint)}: {EmbeddingDeployment}\r\n" +
           $"{nameof(EmbeddingDeployment)}: {EmbeddingDeployment}\r\n" +
           $"{nameof(StorageAccountName)}: {StorageAccountName}\r\n" +
           $"{nameof(ContainerName)}: {ContainerName}\r\n" +
           $"{nameof(BlobServiceEndpoint)}: {BlobServiceEndpoint}\r\n";
}
