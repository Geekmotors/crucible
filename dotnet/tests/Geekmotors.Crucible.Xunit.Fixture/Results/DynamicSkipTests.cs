using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Results;

public sealed class DynamicSkipTests
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public void SkipsWhenARunCallsAssertSkip() => Assert.Skip(FixtureTexts.Skipped);

    [CrucibleFact(Runs = FixtureRuns.Few, SkipExceptions = [typeof(NotSupportedException)])]
    public void SkipsWhenARunThrowsASkipException() => throw new NotSupportedException(FixtureTexts.Problem);
}
