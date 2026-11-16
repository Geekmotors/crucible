using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class AsyncVoidTests
{
    [CrucibleFact(Runs = FixtureRuns.Single)]
    public async void IsRefused() => await Task.Yield();
}
