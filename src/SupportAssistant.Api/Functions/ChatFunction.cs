using System.ClientModel;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using SupportAssistant.Api.Mapping;
using SupportAssistant.Application.Chats;

namespace SupportAssistant.Api.Functions;

public class ChatFunction(IChatService service, IMapper mapper)
{
    protected virtual IChatService Service { get; } = service ?? throw new ArgumentNullException(nameof(service));
    protected virtual IMapper Mapper { get; } = mapper ?? throw new ArgumentNullException(nameof(mapper));

    [Function("Chat")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "chat")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var command = await GetCommandAsync(request, cancellationToken);
        if (command == null)
        {
            return new BadRequestObjectResult(new { error = "Request body is required." });
        }

        if (string.IsNullOrWhiteSpace(command.Question))
        {
            return new BadRequestObjectResult(new { error = "Question is required." });
        }

        if (command.Question.Length > 4000)
        {
            return new BadRequestObjectResult(new { error = "Question is too long." });
        }

        return await RunAsync(command, request, cancellationToken);
    }

    private async Task<IActionResult> RunAsync(ChatCommand command, HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.HandleAsync(command, cancellationToken);
            var response = mapper.MapChatResponse(result, request);
            return new OkObjectResult(response);
        }
        catch (RequestFailedException)
        {
            return ObjectResultFactory.AzRequsetFailed(request);
        }
        catch (ClientResultException)
        {
            return ObjectResultFactory.AiRequestFailed(request);
        }
        catch (OperationCanceledException)
        {
            return ObjectResultFactory.RequestCanceled(request);
        }
        catch
        {
            return ObjectResultFactory.UnexpectedError(request);
        }
    }

    private async Task<ChatCommand?> GetCommandAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await request.ReadFromJsonAsync<ChatCommand>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
