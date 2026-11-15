using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Usage;

public sealed class InjectionTests(ITestOutputHelper output, PromptJudgeInstaller judge)
{
    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task EachRunReceivesTheOutputHelperAndTheAssemblyFixture()
    {
        Assert.NotNull(judge);
        output.WriteLine(FactProbe.Own(nameof(InjectionTests)));
        await FactProbe.AddAndCheckAsync(nameof(InjectionTests));
    }
}
