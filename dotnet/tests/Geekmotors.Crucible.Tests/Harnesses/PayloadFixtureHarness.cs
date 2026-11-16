using System.Text.Json.Nodes;
using CrucibleAI.Assertions;
using CrucibleAI.Runs;

namespace CrucibleAI.Tests.Harnesses;

internal static class PayloadFixtureHarness
{
    public static async Task<JsonNode?> CoherentAsync(JsonNode? input)
    {
        var harness = Harness(input);
        await harness.Coherent.AskAsync(
            harness.Run(),
            new CoherentCall(input!["response"]!.GetValue<string>(), input["claim"]!.GetValue<string>(), CallArguments.Strings(input["callFacts"])));
        return Sent(harness);
    }

    public static async Task<JsonNode?> ExtractAsync(JsonNode? input)
    {
        var harness = Harness(input);
        await harness.Extract.AskAsync(
            harness.Run(),
            new ExtractCall(
                input!["response"]!.GetValue<string>(),
                input["target"]!.GetValue<string>(),
                CallArguments.ExtractType(input["type"]),
                input["mode"]!.GetValue<string>() == "strict",
                CallArguments.Strings(input["callFacts"])));
        return Sent(harness);
    }

    public static JsonNode? Sent(AssertionHarness harness) => Assert.Single(harness.Record.Calls) switch
    {
        VerdictCall verdict => verdict.Reasoning,
        ExtractionCall extraction => extraction.Reasoning,
        var other => throw new InvalidOperationException($"Unexpected judge call {other}."),
    };

    private static AssertionHarness Harness(JsonNode? input)
    {
        var harness = new AssertionHarness();
        harness.Record.AddFacts(CallArguments.Strings(input!["facts"]) ?? []);
        return harness;
    }
}
