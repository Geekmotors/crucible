using CrucibleAI.Judging;
using CrucibleAI.Payloads;
using CrucibleAI.Providers;
using CrucibleAI.Runs;

namespace CrucibleAI.Assertions;

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
        var reply = await run.JudgeAsync(observer => exchange.AskAsync(
                new JudgeRequest(JudgeSchemas.Coherent, prompt), _reader.Read, run.InferenceTimeout, observer, run.Cancellation))
            .ConfigureAwait(false);
        run.Recorded(new VerdictCall(claim, reply.Verdict, reply.Reasoning, actual, facts));
        return reply.Verdict;
    }
}
