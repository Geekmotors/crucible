using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Usage;

public sealed class ConstructorTests
{
    private readonly CrucibleErrorKind? _constructorKind = ErrorKindProbe.Of(() => Crucible.Facts(FixtureTexts.Stray));

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task TheConstructorRunsOutsideTheRun()
    {
        Assert.Equal(CrucibleErrorKind.Usage, _constructorKind);
        await FactProbe.AddAndCheckAsync(nameof(ConstructorTests));
    }
}
