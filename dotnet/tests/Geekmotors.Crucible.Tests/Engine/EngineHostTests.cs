using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;
using CrucibleAI.Tests.Harnesses;
using Microsoft.Extensions.AI;

namespace CrucibleAI.Tests.Engine;

public sealed class EngineHostTests
{
    [Fact]
    public async Task AChatClientJudgesAGateWithoutAConfigFile()
    {
        var harness = new EngineHostHarness();
        harness.Host.UseChatClient(new ScriptedChatClient(ChatClientTexts.Reply), null);

        var error = await Record.ExceptionAsync(harness.GateAsync);

        Assert.Null(error);
    }

    [Fact]
    public async Task AChatClientJudgeStillTakesTheTestDefaultsFromTheConfigFile()
    {
        var harness = new EngineHostHarness(ConfigTexts.HalfOfFourRuns);
        harness.Host.UseChatClient(new ScriptedChatClient(), null);

        var error = await Record.ExceptionAsync(
            () => harness.Host.Engine.GateAsync(
                new RunOptions(),
                context => context.Run <= ConfigTexts.FailingRuns ? throw new InvalidOperationException(EngineTexts.Problem) : Task.CompletedTask,
                TestContext.Current.CancellationToken));

        Assert.Null(error);
    }

    [Fact]
    public async Task WithoutAJudgeTheConfigFileIsRequired()
    {
        var harness = new EngineHostHarness();

        var error = await Assert.ThrowsAsync<CrucibleException>(harness.GateAsync);

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }

    [Fact]
    public void ANullChatClientIsAUsageError()
    {
        var error = Assert.Throws<CrucibleException>(() => new EngineHostHarness().Host.UseChatClient(null, null));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public void AJudgeChosenAfterTheEngineStartedIsAUsageError()
    {
        var harness = new EngineHostHarness();
        _ = harness.Host.Engine;

        var error = Assert.Throws<CrucibleException>(() => harness.Host.UseChatClient(new ScriptedChatClient(), null));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public void ASecondDifferentJudgeIsAUsageError()
    {
        var harness = new EngineHostHarness();
        harness.Host.UseChatClient(new ScriptedChatClient(), null);

        var error = Assert.Throws<CrucibleException>(() => harness.Host.UseChatClient(new ScriptedChatClient(), null));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public void TheSameJudgeAgainIsAccepted()
    {
        var harness = new EngineHostHarness();
        var client = new ScriptedChatClient();
        var options = new ChatOptions();
        harness.Host.UseChatClient(client, options);
        _ = harness.Host.Engine;

        var error = Record.Exception(() => harness.Host.UseChatClient(client, options));

        Assert.Null(error);
    }

    [Fact]
    public void TheInternalJudgeSeamFollowsTheSameRule()
    {
        var harness = new EngineHostHarness();
        _ = harness.Host.Engine;

        var error = Assert.Throws<CrucibleException>(() => harness.Host.UseJudge(new PromptJudgeClient()));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public void AJudgeThatCachesRepliesWarnsOnce()
    {
        var harness = new EngineHostHarness();
        var client = EngineHostHarness.Caching(new ScriptedChatClient());

        harness.Host.UseChatClient(client, null);
        harness.Host.UseChatClient(client, null);

        Assert.Single(harness.Warnings.Lines);
    }

    [Fact]
    public void AClientThatFailsTheCachingCheckIsNotChosen()
    {
        var harness = new EngineHostHarness();
        Assert.ThrowsAny<InvalidOperationException>(() => harness.Host.UseChatClient(new ServiceRefusingChatClient(), null));

        var error = Record.Exception(() => harness.Host.UseChatClient(new ScriptedChatClient(), null));

        Assert.Null(error);
    }

    [Fact]
    public async Task TheDebugTraceNamesTheModelFromTheOptions()
    {
        var harness = new EngineHostHarness(ConfigTexts.DebugVerbosity);
        harness.Host.UseChatClient(new DescribedChatClient(), new ChatOptions { ModelId = ChatClientTexts.ModelId });

        await harness.GateAsync();

        Assert.Contains(ChatClientTexts.ModelId, harness.Output.Text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(ChatClientTexts.BlankModelId)]
    public async Task WithoutAModelInTheOptionsTheDebugTraceNamesTheClientsDefaultModel(string? modelId)
    {
        var harness = new EngineHostHarness(ConfigTexts.DebugVerbosity);
        harness.Host.UseChatClient(new DescribedChatClient(), new ChatOptions { ModelId = modelId });

        await harness.GateAsync();

        Assert.Contains(ChatClientTexts.MetadataModelId, harness.Output.Text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithoutAnyModelTheDebugTraceSaysTheClientChooses()
    {
        var harness = new EngineHostHarness(ConfigTexts.DebugVerbosity);
        harness.Host.UseChatClient(new ScriptedChatClient(ChatClientTexts.Reply), null);

        await harness.GateAsync();

        Assert.Contains(ChatClientTexts.ClientDefaultModel, harness.Output.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void AJudgeWithoutCachingDoesNotWarn()
    {
        var harness = new EngineHostHarness();

        harness.Host.UseChatClient(new ScriptedChatClient(), null);

        Assert.Empty(harness.Warnings.Lines);
    }
}
