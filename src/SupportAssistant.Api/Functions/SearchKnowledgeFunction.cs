using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.Api.Functions;

public class SearchKnowledgeFunction(IKnowledgeRetriever knowledgeRetriever)
{
    protected internal IKnowledgeRetriever KnowledgeRetriever { get; } = knowledgeRetriever;

    [Function("SearchKnowledge")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "knowledge/search")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        string? query = request.Query["q"];

        if (string.IsNullOrWhiteSpace(query))
        {
            return new BadRequestObjectResult(
                new
                {
                    error = "Query parameter 'q' is required."
                });
        }

        var results = await KnowledgeRetriever.SearchAsync(query, top: 5, cancellationToken);

        return new OkObjectResult(results);
    }
}
