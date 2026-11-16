using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class ReturnShapeTests
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public void ASynchronousMethodRunsInsideItsRun() => FactProbe.Add(nameof(ReturnShapeTests));

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async ValueTask AValueTaskMethodRunsInsideItsRun() => await FactProbe.AddAndCheckAsync(nameof(ReturnShapeTests));

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async ValueTask<int> AValueTaskOfTMethodRunsInsideItsRun()
    {
        await FactProbe.AddAndCheckAsync(nameof(ReturnShapeTests));
        return Crucible.Context.Run;
    }

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task<int> ATaskOfTMethodRunsInsideItsRun()
    {
        await FactProbe.AddAndCheckAsync(nameof(ReturnShapeTests));
        return Crucible.Context.Run;
    }
}
