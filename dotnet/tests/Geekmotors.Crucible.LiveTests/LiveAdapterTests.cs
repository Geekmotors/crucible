namespace CrucibleAI.LiveTests;

public sealed class LiveAdapterTests
{
    [CrucibleFact(Runs = LiveTexts.SmokeRuns, Threshold = LiveTexts.EveryRun, Explicit = true, Skip = LiveTexts.NoKey, SkipType = typeof(LiveKey), SkipUnless = nameof(LiveKey.IsSet))]
    public async Task ACrucibleFactJudgesOneCallAgainstTheRealJudge() =>
        Assert.True(await Crucible.CoherentAsync(LiveTexts.Response, LiveTexts.Claim, cancellationToken: TestContext.Current.CancellationToken));

    [CrucibleTheory(Runs = LiveTexts.SmokeRuns, Threshold = LiveTexts.EveryRun, Explicit = true, Skip = LiveTexts.NoKey, SkipType = typeof(LiveKey), SkipUnless = nameof(LiveKey.IsSet))]
    [InlineData(LiveTexts.ArrivalResponse, LiveTexts.ArrivalClaim)]
    public async Task ACrucibleTheoryRowJudgesOneCallAgainstTheRealJudge(string response, string claim) =>
        Assert.True(await Crucible.CoherentAsync(response, claim, cancellationToken: TestContext.Current.CancellationToken));
}
