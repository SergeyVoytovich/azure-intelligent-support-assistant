using Microsoft.Extensions.DependencyInjection;
using SupportAssistant.Application.Chats;

namespace SupportAssistant.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
        => services
            .AddSingleton<IPromptBuilder, PromptBuilder>()
        ;
}
