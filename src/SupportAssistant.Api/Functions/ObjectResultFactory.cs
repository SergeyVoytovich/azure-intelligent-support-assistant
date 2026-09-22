using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SupportAssistant.Api.Functions;

public static class ObjectResultFactory
{
    public static ObjectResult AzRequsetFailed(HttpRequest request)
        => new(
            new
            {
                error = "An Azure service request failed.",
                requestId = request.HttpContext.TraceIdentifier
            })
        {
            StatusCode = StatusCodes.Status502BadGateway
        };

    public static ObjectResult AiRequestFailed(HttpRequest request)
        => new(
            new
            {
                error = "The AI service request failed.",
                requestId = request.HttpContext.TraceIdentifier
            })
        {
            StatusCode = StatusCodes.Status502BadGateway
        };

    public static ObjectResult RequestCanceled(HttpRequest request)
        => new(
            new
            {
                error = "The request was cancelled.",
                requestId = request.HttpContext.TraceIdentifier
            })
        {
            StatusCode = 499
        };

    public static ObjectResult UnexpectedError(HttpRequest request)
        => new(
            new
            {
                error = "An unexpected error occurred.",
                requestId = request.HttpContext.TraceIdentifier
            })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
}
