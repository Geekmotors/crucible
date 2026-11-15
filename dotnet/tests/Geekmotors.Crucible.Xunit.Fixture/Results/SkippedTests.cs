using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Results;

public sealed class SkippedTests
{
    [CrucibleFact(Skip = FixtureTexts.Skipped)]
    public Task NeverRuns() => throw new InvalidOperationException(FixtureTexts.Problem);
}
