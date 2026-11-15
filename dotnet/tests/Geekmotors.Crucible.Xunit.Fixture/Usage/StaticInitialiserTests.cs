using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Usage;

public sealed class StaticInitialiserTests
{
    private static readonly CrucibleErrorKind? StaticKind;

    static StaticInitialiserTests() => StaticKind = ErrorKindProbe.Of(() => Crucible.Facts(FixtureTexts.Stray));

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task TheStaticInitialiserRunsOutsideAnyRun()
    {
        Assert.Equal(CrucibleErrorKind.Usage, StaticKind);
        await FactProbe.AddAndCheckAsync(nameof(StaticInitialiserTests));
    }
}
