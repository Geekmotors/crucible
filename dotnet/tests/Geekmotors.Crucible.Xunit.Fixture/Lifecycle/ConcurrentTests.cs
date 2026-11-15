using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class ConcurrentTests
{
    private static readonly Rendezvous Together = new(FixtureRuns.Several);

    [CrucibleFact(Runs = FixtureRuns.Several)]
    public Task RunsAllAtOnce() => Together.ArriveAsync();
}
