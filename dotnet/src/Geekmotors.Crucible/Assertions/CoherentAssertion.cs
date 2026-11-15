using Geekmotors.Crucible.Judging;
using Geekmotors.Crucible.Payloads;
using Geekmotors.Crucible.Providers;
using Geekmotors.Crucible.Runs;

namespace Geekmotors.Crucible.Assertions;

internal sealed class CoherentAssertion(JudgeExchange exchange)
{
    private readonly CoherentReplyReader _reader = new();

    public async Task<bool> AskAsync(AssertionRun run, CoherentCall call)
    {
        if (call.Actual is not { } actual || call.Expectation is not { } claim)
        {
            throw CrucibleException.Usage("Coherent needs a string actual and a string expectation.");
        }
        IReadOnlyList<string> facts = [.. run.Record.Facts, .. FactList.Optional(call.Facts)];
        var prompt = PayloadAssembler.Assemble(facts, [new PayloadSection("RESPONSE", actual), new PayloadSection("CLAIM", claim)]);
        var reply = await exchange.AskAsync(new JudgeRequest(JudgeSchemas.Coherent, prompt), _reader.Read, run.InferenceTimeout, run.Cancellation)
            .ConfigureAwait(false);
        run.Record.Record(new VerdictCall(claim, reply.Verdict, reply.Reasoning, actual, facts));
        return reply.Verdict;
    }
}
