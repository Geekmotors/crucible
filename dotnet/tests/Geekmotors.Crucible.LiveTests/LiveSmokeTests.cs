namespace CrucibleAI.LiveTests;

public sealed class LiveSmokeTests
{
    [Fact(Explicit = true)]
    public async Task JudgesOneCoherentAndOneExtractCallAgainstTheRealJudge()
    {
        Assert.SkipWhen(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(LiveTexts.KeyVariable)), LiveTexts.NoKey);

        await Crucible.Gate(new RunOptions { Runs = LiveTexts.SmokeRuns, Threshold = LiveTexts.EveryRun }, async context =>
        {
            Assert.True(await Crucible.CoherentAsync(LiveTexts.Response, LiveTexts.Claim, cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(
                LiveTexts.DeliveryHour,
                await context.ExtractAsync<int?>(LiveTexts.Response, LiveTexts.Target, cancellationToken: TestContext.Current.CancellationToken));
        });
    }
}
