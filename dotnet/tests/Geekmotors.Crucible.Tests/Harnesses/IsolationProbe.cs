using CrucibleAI.Tests.Constants;

namespace CrucibleAI.Tests.Harnesses;

internal sealed class IsolationProbe
{
    public async Task SeesOnlyItsOwnAsync(string gate, string other, CrucibleContext context, Rendezvous together)
    {
        Crucible.Facts(EngineHarness.Fact(gate, context.Run));
        await together.ArriveAsync();

        var prompt = await EngineHarness.PromptAsync();

        Assert.Contains(EngineHarness.Fact(gate, context.Run), prompt, StringComparison.Ordinal);
        Assert.All(Enumerable.Range(1, EngineRuns.Few), run => Assert.DoesNotContain(EngineHarness.Fact(other, run), prompt, StringComparison.Ordinal));
    }

    public async Task AddFactLaterAsync(string fact)
    {
        await Task.Yield();
        Crucible.Facts(fact);
    }

    public async Task<CrucibleContext> ContextLaterAsync()
    {
        await Task.Yield();
        return Crucible.Context;
    }
}
