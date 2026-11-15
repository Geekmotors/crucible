using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class FailingConstructorTests
{
    public FailingConstructorTests() => throw new InvalidOperationException(FixtureTexts.Problem);

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public Task EveryRunFailsWhenItsInstanceCannotBeBuilt() => Task.CompletedTask;
}
