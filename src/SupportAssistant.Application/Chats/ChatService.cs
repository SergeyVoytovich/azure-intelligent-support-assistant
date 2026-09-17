namespace SupportAssistant.Application.Chats;

public class ChatService(IAnswerGenerator answerGenerator) : IChatService
{
    protected virtual IAnswerGenerator  AnswerGenerator { get; }
        = answerGenerator ??  throw new ArgumentNullException(nameof(answerGenerator));

    public async Task<ChatResult> HandleAsync(ChatCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command.Question);

        var answer = await AnswerGenerator.GenerateAsync(command.Question, cancellationToken);

        return ChatResult.New(answer);
    }
}
