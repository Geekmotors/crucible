using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class StaticMethodTests
{
    private static readonly CrucibleErrorKind? InitialiserKind = ErrorKindProbe.Of(() => Crucible.Facts(FixtureTexts.Stray));

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public static async Task RunsWithoutAnInstanceAfterItsStaticInitialiser()
    {
        Assert.Equal(CrucibleErrorKind.Usage, InitialiserKind);
        await FactProbe.AddAndCheckAsync(nameof(StaticMethodTests));
    }
}
