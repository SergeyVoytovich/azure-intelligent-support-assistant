using Microsoft.Extensions.DependencyInjection;
using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Escalation;

namespace SupportAssistant.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
        => services
            .AddSingleton<IPromptBuilder, PromptBuilder>()
            .AddSingleton<IEscalationPolicy, EscalationPolicy>()
        ;
}
