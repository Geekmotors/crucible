using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Settings;

[Crucible(Runs = FixtureRuns.ClassRuns)]
public sealed class ClassDefaultsTests
{
    private static readonly Rendezvous ClassRuns = new(FixtureRuns.ClassRuns);
    private static readonly Rendezvous MethodRuns = new(FixtureRuns.Few);

    [CrucibleFact]
    public Task TheClassSetsTheRuns()
    {
        Assert.InRange(Crucible.Context.Run, 1, FixtureRuns.ClassRuns);
        return ClassRuns.ArriveAsync();
    }

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public Task TheMethodOverridesTheClass()
    {
        Assert.InRange(Crucible.Context.Run, 1, FixtureRuns.Few);
        return MethodRuns.ArriveAsync();
    }
}
