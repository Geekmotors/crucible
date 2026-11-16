using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Usage;

public sealed class ClassFixtureTests(ClassFixtureProbe probe) : IClassFixture<ClassFixtureProbe>
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task TheClassFixtureIsBuiltOutsideAnyRun()
    {
        Assert.Equal(CrucibleErrorKind.Usage, probe.Kind);
        await FactProbe.AddAndCheckAsync(nameof(ClassFixtureTests));
    }
}
