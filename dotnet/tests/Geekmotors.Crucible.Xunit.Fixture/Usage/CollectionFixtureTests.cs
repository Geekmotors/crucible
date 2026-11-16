using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Usage;

[Collection(FixtureCollections.Probed)]
public sealed class CollectionFixtureTests(CollectionFixtureProbe probe)
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task TheCollectionFixtureIsBuiltOutsideAnyRun()
    {
        Assert.Equal(CrucibleErrorKind.Usage, probe.Kind);
        await FactProbe.AddAndCheckAsync(nameof(CollectionFixtureTests));
    }
}
