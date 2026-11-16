using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class FailingDisposalTests : IDisposable
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public Task EveryRunFailsWhenItsInstanceCannotBeDisposed() => Task.CompletedTask;

    public void Dispose() => throw new InvalidOperationException(FixtureTexts.Problem);
}
