using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Isolation;

[Collection(FixtureCollections.Alpha)]
public sealed class CollectionAlphaTests
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task MeetsTheOtherCollectionAndSeesOnlyItsOwnFacts()
    {
        FactProbe.Add(nameof(CollectionAlphaTests));
        await Meetings.Collections.ArriveAsync();
        await FactProbe.SeesOnlyItsOwnAsync(nameof(CollectionAlphaTests));
    }
}
