using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class SequentialTests
{
    private static int _active;

    [CrucibleFact(Runs = FixtureRuns.Several, Sequential = true)]
    public async Task RunsOneAtATime()
    {
        Assert.Equal(FixtureRuns.OneAtATime, Interlocked.Increment(ref _active));
        await Task.Delay(FixtureRuns.Pause, Crucible.Context.CancellationToken);
        Interlocked.Decrement(ref _active);
    }
}
