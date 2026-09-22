using System.ClientModel;
using OpenAI.Chat;

namespace SupportAssistant.Infrastructure.Chats;

public static class ChatClientExtensions
{
    public static Task<ClientResult<ChatCompletion>> CompleteChatAsync(this ChatClient client, string promt, CancellationToken cancellationToken = default)
        => client.CompleteChatAsync([new UserChatMessage(promt)], cancellationToken: cancellationToken);
}
