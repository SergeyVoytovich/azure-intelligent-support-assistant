using System.ClientModel;
using System.Text.Json;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SupportAssistant.Api.Mapping;
using SupportAssistant.Application.Chats;

namespace SupportAssistant.Api.Functions;

public class ChatFunction
{
    private static readonly Action<ILogger, Exception?> LogUnexpectedError =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1001, "UnexpectedChatError"),
            "Unexpected error while processing chat request.");

    protected virtual IChatService Service { get; }
    protected virtual IMapper Mapper { get; }
    protected virtual ILogger<ChatFunction> Logger { get; }

    public ChatFunction(IChatService service, IMapper mapper, ILogger<ChatFunction> logger)
    {
        Service = service ?? throw new ArgumentNullException(nameof(service));
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("Chat")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        ChatCommand? command;

        try
        {
            command = await request.ReadFromJsonAsync<ChatCommand>(
                cancellationToken);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult(
                new { error = "Invalid JSON request body." });
        }
        catch (BadHttpRequestException)
        {
            return new BadRequestObjectResult(
                new { error = "Invalid request body." });
        }

        if (command == null)
        {
            return new BadRequestObjectResult(
                new { error = "Request body is required." });
        }

        if (string.IsNullOrWhiteSpace(command.Question))
        {
            return new BadRequestObjectResult(
                new { error = "Question is required." });
        }

        if (command.Question.Length > 4000)
        {
            return new BadRequestObjectResult(
                new { error = "Question is too long." });
        }

        return await RunAsync(
            command,
            request,
            cancellationToken);
    }

    private async Task<IActionResult> RunAsync(
        ChatCommand command,
        HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await Service.HandleAsync(
                command,
                cancellationToken);

            var response = Mapper.MapChatResponse(
                result,
                request);

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
        catch (Exception ex)
        {
            LogUnexpectedError(Logger, ex);

            return ObjectResultFactory.UnexpectedError(request);
        }
    }
}
