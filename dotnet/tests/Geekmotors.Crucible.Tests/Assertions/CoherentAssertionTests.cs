using CrucibleAI.Assertions;
using CrucibleAI.Runs;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;
using CrucibleAI.Tests.Harnesses;

namespace CrucibleAI.Tests.Assertions;

public sealed class CoherentAssertionTests
{
    [Fact]
    public async Task RecordsTheVerdictWithTheRunFactsThenTheCallFacts()
    {
        var harness = new AssertionHarness();
        harness.Record.AddFacts([AssertionTexts.RunFact]);

        var verdict = await harness.Coherent.AskAsync(
            harness.Run(), new CoherentCall(AssertionTexts.Response, AssertionTexts.Claim, [AssertionTexts.CallFact]));

        var call = Assert.IsType<VerdictCall>(Assert.Single(harness.Record.Calls));
        Assert.Equal((true, AssertionTexts.Claim, AssertionTexts.Response), (verdict, call.Claim, call.Response));
        Assert.Equal([AssertionTexts.RunFact, AssertionTexts.CallFact], call.Facts);
    }

    [Fact]
    public async Task SendsTheCoherentSchemaAsTheSystemMessage()
    {
        var harness = new AssertionHarness(new EchoSystemJudgeClient());

        await harness.Coherent.AskAsync(harness.Run(), new CoherentCall(AssertionTexts.Response, AssertionTexts.Claim, null));

        Assert.Equal(RepositorySchemas.Text(AssertionTexts.CoherentSchema), Assert.IsType<VerdictCall>(Assert.Single(harness.Record.Calls)).Reasoning);
    }

    [Fact]
    public async Task CallFactsAreNotAddedToTheRun()
    {
        var harness = new AssertionHarness();

        await harness.Coherent.AskAsync(harness.Run(), new CoherentCall(AssertionTexts.Response, AssertionTexts.Claim, [AssertionTexts.CallFact]));

        Assert.Empty(harness.Record.Facts);
    }

    [Fact]
    public async Task FactsAddedAfterTheCallStartedDoNotReachIt()
    {
        var release = new TaskCompletionSource();
        var harness = new AssertionHarness(release: release.Task);
        var asked = harness.Coherent.AskAsync(harness.Run(), new CoherentCall(AssertionTexts.Response, AssertionTexts.Claim, null));

        harness.Record.AddFacts([AssertionTexts.LateFact]);
        release.SetResult();
        await asked;

        Assert.Empty(Assert.IsType<VerdictCall>(Assert.Single(harness.Record.Calls)).Facts);
    }

    [Fact]
    public async Task AMissingActualIsAUsageError()
    {
        var harness = new AssertionHarness();

        var error = await Assert.ThrowsAsync<CrucibleException>(
            () => harness.Coherent.AskAsync(harness.Run(), new CoherentCall(null, AssertionTexts.Claim, null)));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public async Task ANullCallFactIsAUsageError()
    {
        var harness = new AssertionHarness();

        var error = await Assert.ThrowsAsync<CrucibleException>(
            () => harness.Coherent.AskAsync(harness.Run(), new CoherentCall(AssertionTexts.Response, AssertionTexts.Claim, [null])));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public async Task AJudgeCallThatKeptFailingIsRecordedWithItsProviderAttempts()
    {
        var failing = HttpAnswers.Json(AssertionTexts.ServerError, null);
        var harness = new AssertionHarness(new ProviderHarness(JudgeSettings.OpenRouterConfig).Client(failing, failing, failing));

        await Assert.ThrowsAsync<CrucibleException>(
            () => harness.Coherent.AskAsync(harness.Run(), new CoherentCall(AssertionTexts.Response, AssertionTexts.Claim, null)));

        Assert.Equal(AssertionTexts.ProviderAttempts, Assert.IsType<ErroredCall>(Assert.Single(harness.Record.Calls)).Attempts);
    }

    [Fact]
    public async Task AJudgeCallWithoutAKeyIsNotRecorded()
    {
        var noKey = new ProviderHarness(JudgeSettings.OpenRouterConfig, new Dictionary<string, string>(StringComparer.Ordinal));
        var harness = new AssertionHarness(noKey.Client());

        await Assert.ThrowsAsync<CrucibleException>(
            () => harness.Coherent.AskAsync(harness.Run(), new CoherentCall(AssertionTexts.Response, AssertionTexts.Claim, null)));

        Assert.Empty(harness.Record.Calls);
    }
}
