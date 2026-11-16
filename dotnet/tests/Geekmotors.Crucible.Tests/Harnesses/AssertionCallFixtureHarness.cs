using System.Text.Json.Nodes;
using CrucibleAI.Assertions;

namespace CrucibleAI.Tests.Harnesses;

internal static class AssertionCallFixtureHarness
{
    private const string FactsKey = "facts";

    private static readonly HashSet<string> CoherentKeys = new(StringComparer.Ordinal) { FactsKey };
    private static readonly HashSet<string> ExtractKeys = new(StringComparer.Ordinal) { "as", "strict", FactsKey };

    public static async Task<JsonNode?> CallAsync(JsonNode? input)
    {
        var strict = input!["strict"]?.GetValue<bool>() ?? false;
        var harness = new AssertionHarness();
        foreach (var facts in input["facts"]?.AsArray() ?? [])
        {
            harness.Record.AddFacts(CallArguments.Facts(facts));
        }
        var arguments = input["args"]!.AsArray();
        if (arguments.Count < 2)
        {
            throw new InexpressibleInputException(arguments.ToJsonString());
        }
        if (input["assertion"]!.GetValue<string>() == "coherent")
        {
            var options = CallArguments.Options(arguments, CoherentKeys);
            await harness.Coherent.AskAsync(
                harness.Run(strict),
                new CoherentCall(CallArguments.Text(arguments[0]), CallArguments.Text(arguments[1]), CallFacts(options)));
        }
        else
        {
            var options = CallArguments.Options(arguments, ExtractKeys);
            await harness.Extract.AskAsync(
                harness.Run(strict),
                new ExtractCall(
                    CallArguments.Text(arguments[0]),
                    CallArguments.Text(arguments[1]),
                    CallArguments.ExtractType(options?["as"]),
                    CallArguments.Strict(options?["strict"]),
                    CallFacts(options)));
        }
        return PayloadFixtureHarness.Sent(harness);
    }

    private static IReadOnlyList<string?>? CallFacts(JsonObject? options) =>
        options?[FactsKey] is { } facts ? CallArguments.Facts(facts) : null;
}
