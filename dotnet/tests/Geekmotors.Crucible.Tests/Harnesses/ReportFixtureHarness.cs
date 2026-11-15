using System.Text.Json.Nodes;
using Geekmotors.Crucible.Reports;
using Geekmotors.Crucible.Runs;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class ReportFixtureHarness
{
    public static JsonNode? Render(JsonNode? input)
    {
        var settings = input!["settings"]!;
        var renderer = new ReportRenderer(new ReportSettings(settings["excerptChars"]!.GetValue<int>(), RunRecordJson.Level(settings["verbosity"])));
        var test = new TestRecord(
            input["runs"]!.GetValue<int>(),
            input["threshold"]!.GetValue<double>(),
            Ending(input["ending"]!),
            input["results"]!.AsArray().Select(RunRecordJson.Result).ToArray());
        return renderer.Render(test);
    }

    private static TestEnding Ending(JsonNode ending) => ending["kind"]!.GetValue<string>() switch
    {
        "aborted" => new AbortedEnding(ending["fatalRun"]!.GetValue<int>()),
        "expired" => new ExpiredEnding(ending["deadlineMs"]!.GetValue<long>()),
        _ => new VerdictEnding(),
    };
}
