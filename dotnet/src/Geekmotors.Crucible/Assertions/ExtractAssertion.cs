using Geekmotors.Crucible.Judging;
using Geekmotors.Crucible.Payloads;
using Geekmotors.Crucible.Providers;
using Geekmotors.Crucible.Runs;

namespace Geekmotors.Crucible.Assertions;

internal sealed class ExtractAssertion(JudgeExchange exchange)
{
    public async Task<object?> AskAsync(AssertionRun run, ExtractCall call)
    {
        if (call.Text is not { } text || call.Description is not { } description)
        {
            throw CrucibleException.Usage("Extract needs a string text and a string description.");
        }
        var type = ExtractTypes.For(call.As);
        var mode = (call.Strict ?? run.Strict) ? ExtractMode.Strict : ExtractMode.Lenient;
        IReadOnlyList<string> facts = [.. run.Record.Facts, .. FactList.Optional(call.Facts)];
        var prompt = PayloadAssembler.Assemble(
            facts,
            [
                new PayloadSection("RESPONSE", text),
                new PayloadSection("TARGET", description),
                new PayloadSection("TYPE", ExtractWords.Of(type)),
                new PayloadSection("MODE", ExtractWords.Of(mode)),
            ]);
        var reply = await exchange.AskAsync(
                new JudgeRequest(JudgeSchemas.Extract, prompt), new ExtractReplyReader(type).Read, run.InferenceTimeout, run.Cancellation)
            .ConfigureAwait(false);
        run.Record.Record(new ExtractionCall(description, type, mode, reply.Value, reply.Reasoning, text, facts));
        return reply.Value;
    }
}
