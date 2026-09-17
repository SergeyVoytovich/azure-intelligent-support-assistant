using AutoMapper;
using Microsoft.AspNetCore.Http;
using SupportAssistant.Api.Contracts.Chats;
using SupportAssistant.Application.Chats;

namespace SupportAssistant.Api.Mapping;

public static class DtoProfileExtensions
{
    public static ChatResponse MapChatResponse(this IMapper mapper, ChatResult result, HttpRequest request)
    {
        var response = mapper.Map<ChatResponse>(result);
        response.RequestId = request.HttpContext.TraceIdentifier;
        return response;
        ;
    }
}
