using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Isolation;

[Collection(FixtureCollections.Beta)]
public sealed class CollectionBetaTests
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task MeetsTheOtherCollectionAndSeesOnlyItsOwnFacts()
    {
        FactProbe.Add(nameof(CollectionBetaTests));
        await Meetings.Collections.ArriveAsync();
        await FactProbe.SeesOnlyItsOwnAsync(nameof(CollectionBetaTests));
    }
}
