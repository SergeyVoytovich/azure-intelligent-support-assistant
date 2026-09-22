using AutoMapper;
using Azure.AI.DocumentIntelligence;
using SupportAssistant.Application.Documents;

namespace SupportAssistant.Infrastructure.Documents;

public class DocumentAnalyzer(DocumentIntelligenceClient client, IMapper mapper) : IDocumentAnalyzer
{
    protected virtual DocumentIntelligenceClient  Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
    protected virtual IMapper Mapper { get; } = mapper ??  throw new ArgumentNullException(nameof(mapper));

    public async Task<DocumentAnalysis> AnalyzeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var operation = await Client.AnalyzePrebuiltAsync(stream, cancellationToken);
        var result = Mapper.Map<DocumentAnalysis>(operation);
        return result;
    }
}
