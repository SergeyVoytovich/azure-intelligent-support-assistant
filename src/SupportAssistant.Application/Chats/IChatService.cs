namespace SupportAssistant.Application.Chats;

public interface IChatService
{
    Task<ChatResult> HandleAsync(ChatCommand command, CancellationToken cancellationToken = default);
}
