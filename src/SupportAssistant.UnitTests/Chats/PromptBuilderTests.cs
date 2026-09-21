using SupportAssistant.Application.Chats;
using SupportAssistant.Application.Knowledge;

namespace SupportAssistant.UnitTests.Chat;

public sealed class PromptBuilderTests
{
    private readonly PromptBuilder _sut = new();

    [Fact]
    public void Build_ShouldContainUserQuestion()
    {
        const string question = "How long is the warranty?";

        var prompt = _sut.Build(question, []);

        Assert.Contains(question, prompt);
    }

    [Fact]
    public void Build_ShouldContainKnowledgeContent()
    {
        var knowledge = new[]
        {
            new KnowledgeSearchResult(
                "The warranty period is 24 months.",
                "warranty.pdf",
                1,
                0.9)
        };

        var prompt = _sut.Build("How long is the warranty?", knowledge);

        Assert.Contains("The warranty period is 24 months.", prompt);
    }

    [Fact]
    public void Build_ShouldContainSource()
    {
        var knowledge = new[]
        {
            new KnowledgeSearchResult(
                "Some content",
                "warranty.pdf",
                1,
                0.9)
        };

        var prompt = _sut.Build("Question", knowledge);

        Assert.Contains("warranty.pdf", prompt);
    }

    [Fact]
    public void Build_ShouldContainAllKnowledgeChunks()
    {
        var knowledge = new[]
        {
            new KnowledgeSearchResult(
                "Shipping takes 2-4 business days.",
                "shipping.pdf",
                1,
                0.9),

            new KnowledgeSearchResult(
                "Express shipping costs EUR 9.99.",
                "shipping.pdf",
                1,
                0.8)
        };

        var prompt = _sut.Build("Tell me about shipping.", knowledge);

        Assert.Contains("Shipping takes 2-4 business days.", prompt);

        Assert.Contains("Express shipping costs EUR 9.99.", prompt);
    }

    [Fact]
    public void Build_ShouldContainGroundingInstructions()
    {
        var prompt = _sut.Build("Question", []);

        Assert.Contains("using only the provided knowledge base context", prompt);

        Assert.Contains("do not have enough information", prompt);
    }

    [Fact]
    public void Build_ShouldThrow_WhenQuestionIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => _sut.Build("", []));
    }

    [Fact]
    public void Build_ShouldThrow_WhenQuestionIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => _sut.Build("   ", []));
    }
}
