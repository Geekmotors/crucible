using System.Text.Json.Nodes;
using Geekmotors.Crucible.Verdicts;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class VerdictFixtureHarness
{
    private static readonly VerdictCalculator Calculator = new();

    private static readonly Dictionary<string, RunStatus> Statuses = new(StringComparer.Ordinal)
    {
        ["passed"] = RunStatus.Passed,
        ["failed"] = RunStatus.Failed,
        ["errored"] = RunStatus.Errored,
        ["discarded"] = RunStatus.Discarded,
    };

    private static readonly Dictionary<VerdictOutcome, string> Outcomes = new()
    {
        [VerdictOutcome.Passed] = "passed",
        [VerdictOutcome.Failed] = "failed",
        [VerdictOutcome.Errored] = "errored",
    };

    public static JsonNode? Compute(JsonNode? input)
    {
        var threshold = input!["threshold"]!.GetValue<double>();
        var statuses = input["statuses"]!.AsObject()
            .SelectMany(entry => Enumerable.Repeat(Statuses[entry.Key], entry.Value!.GetValue<int>()))
            .ToList();
        var verdict = Calculator.Compute(threshold, statuses);
        return new JsonObject
        {
            ["outcome"] = Outcomes[verdict.Outcome],
            ["need"] = verdict.Need,
            ["passed"] = verdict.Passed,
            ["failed"] = verdict.Failed,
            ["errored"] = verdict.Errored,
        };
    }
}
