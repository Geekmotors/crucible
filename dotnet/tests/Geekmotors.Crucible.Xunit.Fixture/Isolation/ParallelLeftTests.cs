using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Isolation;

public sealed class ParallelLeftTests
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task MeetsTheOtherClassAndSeesOnlyItsOwnFacts()
    {
        FactProbe.Add(nameof(ParallelLeftTests));
        await Meetings.ParallelClasses.ArriveAsync();
        await FactProbe.SeesOnlyItsOwnAsync(nameof(ParallelLeftTests));
    }
}
