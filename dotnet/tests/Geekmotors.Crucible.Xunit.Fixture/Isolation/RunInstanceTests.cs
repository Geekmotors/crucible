using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Isolation;

public sealed class RunInstanceTests
{
    private int _bodies;

    [CrucibleFact(Runs = FixtureRuns.Several)]
    public async Task EachRunHasItsOwnInstanceAndFacts()
    {
        Assert.Equal(1, Interlocked.Increment(ref _bodies));
        await FactProbe.AddAndCheckAsync(nameof(RunInstanceTests));
        Assert.Equal(1, Volatile.Read(ref _bodies));
    }
}
