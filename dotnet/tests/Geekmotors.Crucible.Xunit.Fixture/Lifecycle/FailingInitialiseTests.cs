using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class FailingInitialiseTests : IAsyncLifetime
{
    public ValueTask InitializeAsync() => throw new InvalidOperationException(FixtureTexts.Problem);

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public Task EveryRunFailsWhenItsInstanceCannotBeInitialised() => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
