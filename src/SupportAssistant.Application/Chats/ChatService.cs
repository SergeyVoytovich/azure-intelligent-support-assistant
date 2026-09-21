using SupportAssistant.Application.Escalation;
using SupportAssistant.Application.Language;

namespace SupportAssistant.Application.Chats;

public  class ChatService
    (IAnswerGenerator answerGenerator, ITextAnalyzer textAnalyzer, IEscalationPolicy escalationPolicy)
    : IChatService
{
    protected virtual IEscalationPolicy Policy { get; } = escalationPolicy;
    protected virtual  ITextAnalyzer TextAnalyzer { get; } = textAnalyzer;
    protected virtual  IAnswerGenerator AnswerGenerator { get; } = answerGenerator;

    public async Task<ChatResult> HandleAsync(ChatCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var analysis = await TextAnalyzer.AnalyzeAsync(command.Question, cancellationToken);

        var escalation = Policy.Evaluate(command.Question, analysis);

        var generated = await AnswerGenerator.GenerateAsync(command.Question, cancellationToken);

        return new ChatResult(
            generated.Answer,
            generated.Sources,
            escalation.Required);
    }
}
