using CrucibleAI.Judging;
using CrucibleAI.Payloads;
using CrucibleAI.Providers;
using CrucibleAI.Runs;

namespace CrucibleAI.Assertions;

internal sealed class ExtractAssertion(JudgeExchange exchange)
{
    public async Task<object?> AskAsync(AssertionRun run, ExtractCall call)
    {
        if (call.Text is not { } text || call.Description is not { } description)
        {
            throw CrucibleException.Usage("Extract needs a string text and a string description.");
        }
        var target = ExtractTypes.For(call.As);
        var mode = (call.Strict ?? run.Strict) ? ExtractMode.Strict : ExtractMode.Lenient;
        IReadOnlyList<string> facts = [.. run.Record.Facts, .. FactList.Optional(call.Facts)];
        var prompt = PayloadAssembler.Assemble(
            facts,
            [
                new PayloadSection("RESPONSE", text),
                new PayloadSection("TARGET", description),
                new PayloadSection("TYPE", ExtractWords.Of(target.Type)),
                new PayloadSection("MODE", ExtractWords.Of(mode)),
            ]);
        var reader = new ExtractReplyReader(target);
        var reply = await run.JudgeAsync(observer => exchange.AskAsync(
                new JudgeRequest(JudgeSchemas.Extract, prompt), reader.Read, run.InferenceTimeout, observer, run.Cancellation))
            .ConfigureAwait(false);
        run.Recorded(new ExtractionCall(description, target.Type, mode, reply.Value, reply.Reasoning, text, facts));
        return target.Converted(reply.Value);
    }
}
