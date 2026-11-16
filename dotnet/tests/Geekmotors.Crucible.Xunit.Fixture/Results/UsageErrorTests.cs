using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Results;

public sealed class UsageErrorTests
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public void FailsWithTheUsageErrorARunThrows() => Crucible.Facts(FixtureTexts.Stray, null!);
}
