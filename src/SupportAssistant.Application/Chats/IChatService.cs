namespace SupportAssistant.Application.Chats;

public interface IChatService
{
    Task<ChatResult> Handleasync(ChatCommand command, CancellationToken cancellationToken = default);
}
