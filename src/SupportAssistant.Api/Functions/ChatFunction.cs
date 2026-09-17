using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SupportAssistant.Api.Contracts.Chats;
using SupportAssistant.Api.Mapping;
using SupportAssistant.Application.Chats;

namespace SupportAssistant.Api.Functions;

public class ChatFunction(IChatService service, IMapper mapper)
{
    protected virtual IChatService Service { get; } = service ?? throw new ArgumentNullException(nameof(service));
    protected virtual IMapper Mapper { get; } = mapper ?? throw new ArgumentNullException(nameof(mapper));

    [Function("Chat")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat")] HttpRequest request,
        CancellationToken  cancellationToken)
    {
        var body = await request.ReadFromJsonAsync<ChatRequest>(cancellationToken);
        if (string.IsNullOrWhiteSpace(body?.Question))
        {
            return new BadRequestObjectResult(new { error = "Questions is required" });
        }

        var result = await Service.HandleAsync(new ChatCommand(body.Question), cancellationToken);
        var response = mapper.MapChatResponse(result, request);
        return new OkObjectResult(response);

    }
}
