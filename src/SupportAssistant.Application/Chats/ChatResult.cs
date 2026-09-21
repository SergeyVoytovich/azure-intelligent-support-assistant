namespace SupportAssistant.Application.Chats;

public sealed record ChatResult
    (
        string Answer,
        IReadOnlyCollection<string> Sources,
        bool EscalationRequired
    )
{
    public static ChatResult New(string answer)
    {
        return New(answer, false);
    }

    public static ChatResult New(string answer, bool escalationRequired)
        => New(answer, [], escalationRequired);

    public static ChatResult New(string answer,  IReadOnlyCollection<string> Sources, bool escalationRequired)
    {
        return new ChatResult(answer, Sources, escalationRequired);
    }
}
