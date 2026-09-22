using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SupportAssistant.Infrastructure.Ingestion;

namespace SupportAssistant.Api.Functions;

public sealed class IngestKnowledgeFunction(
    KnowledgeIngestionService ingestionService)
{
    [Function("IngestKnowledge")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(
            AuthorizationLevel.Function,
            // AuthorizationLevel.Anonymous, // only for local using
            "post",
            Route = "knowledge/ingest")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        IngestionResult result = await ingestionService.RunAsync(cancellationToken);

        return new OkObjectResult(result);
    }
}
