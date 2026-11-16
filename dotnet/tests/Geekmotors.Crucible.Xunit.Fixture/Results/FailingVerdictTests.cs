using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Results;

public sealed class FailingVerdictTests
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public Task FailsWithTheReport() => throw new InvalidOperationException(FixtureTexts.Problem);
}
