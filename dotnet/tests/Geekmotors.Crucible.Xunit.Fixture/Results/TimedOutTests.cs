using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Results;

public sealed class TimedOutTests
{
    [CrucibleFact(Runs = FixtureRuns.Single, TestTimeout = FixtureRuns.LongTestTimeoutMs, Timeout = FixtureRuns.XunitTimeoutMs)]
    public async Task IsCancelledByXunitsTimeout()
    {
        Assert.False(TestContext.Current.CancellationToken.IsCancellationRequested);
        var run = Crucible.Context.CancellationToken;
        run.Register(() => Signals.TimedOutRunCancelled.TrySetResult());
        await Task.Delay(Timeout.Infinite, run);
    }
}
