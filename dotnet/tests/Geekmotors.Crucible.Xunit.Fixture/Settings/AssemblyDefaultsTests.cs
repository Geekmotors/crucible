using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Settings;

public sealed class AssemblyDefaultsTests
{
    private static readonly Rendezvous AssemblyRuns = new(FixtureRuns.AssemblyRuns);

    [CrucibleFact]
    public Task TheAssemblySetsTheRuns()
    {
        Assert.InRange(Crucible.Context.Run, 1, FixtureRuns.AssemblyRuns);
        return AssemblyRuns.ArriveAsync();
    }
}
