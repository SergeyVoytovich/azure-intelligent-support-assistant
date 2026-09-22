using System.Text;
using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.Application.Chats;

public sealed class PromptBuilder : IPromptBuilder
{
    public string Build(
        string userQuestion,
        IReadOnlyCollection<KnowledgeSearchResult> knowledge)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userQuestion);

        var builder = new StringBuilder();

        AppendMain(builder);
        AppendContext(builder, knowledge);
        AppendQuestion(builder, userQuestion);

        return builder.ToString();
    }

    private static void AppendMain(StringBuilder builder)
    {
        builder.AppendLine("You are a customer support assistant for SVoy Electronics GmbH.");
        builder.AppendLine("Answer the customer's question using only the provided knowledge base context.");
        builder.AppendLine("If the context does not contain enough information, say that you do not have enough information.");
        builder.AppendLine("Do not invent policies, prices, delivery times, warranty conditions, or other facts.");
        builder.AppendLine();
    }

    private static void AppendContext(
        StringBuilder builder,
        IReadOnlyCollection<KnowledgeSearchResult> knowledge)
    {
        builder.AppendLine("Knowledge base context:");

        foreach (var item in knowledge)
        {
            builder.AppendLine();
            builder.Append("Source: ");
            builder.AppendLine(item.Source);
            builder.AppendLine(item.Content);
        }

        builder.AppendLine();
    }

    private static void AppendQuestion(
        StringBuilder builder,
        string userQuestion)
    {
        builder.AppendLine("Customer question:");
        builder.AppendLine(userQuestion);
    }
}
