using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Isolation;

public sealed class SameClassTests
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public Task TheFirstTestSeesOnlyItsOwnFacts() => FactProbe.AddAndCheckAsync(nameof(TheFirstTestSeesOnlyItsOwnFacts));

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public Task TheSecondTestSeesOnlyItsOwnFacts() => FactProbe.AddAndCheckAsync(nameof(TheSecondTestSeesOnlyItsOwnFacts));
}
