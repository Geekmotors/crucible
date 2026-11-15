using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Isolation;

public sealed class ParallelRightTests
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task MeetsTheOtherClassAndSeesOnlyItsOwnFacts()
    {
        FactProbe.Add(nameof(ParallelRightTests));
        await Meetings.ParallelClasses.ArriveAsync();
        await FactProbe.SeesOnlyItsOwnAsync(nameof(ParallelRightTests));
    }
}
