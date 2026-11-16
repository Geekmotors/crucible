using System.Text.Json.Nodes;
using Geekmotors.Crucible.Tests.Fakes;
using Geekmotors.Crucible.Tracing;
using Geekmotors.Crucible.Verdicts;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class TraceFixtureHarness
{
    private static readonly Dictionary<string, VerdictOutcome> Outcomes = new(StringComparer.Ordinal)
    {
        ["passed"] = VerdictOutcome.Passed,
        ["failed"] = VerdictOutcome.Failed,
        ["errored"] = VerdictOutcome.Errored,
    };

    private static readonly Dictionary<string, ProviderFailureNext> Nexts = new(StringComparer.Ordinal)
    {
        ["retry"] = ProviderFailureNext.Retry,
        ["fatal"] = ProviderFailureNext.Fatal,
        ["exhausted"] = ProviderFailureNext.Exhausted,
        ["aborted"] = ProviderFailureNext.Aborted,
    };

    public static JsonNode? Trace(JsonNode? input)
    {
        var settings = input!["settings"]!;
        var output = new RecordingTraceOutput();
        var trace = new DebugTrace(
            new TraceSettings(
                RunRecordJson.Level(settings["verbosity"]),
                settings["provider"]!.GetValue<string>(),
                settings["model"]!.GetValue<string>(),
                settings["meta"]?.AsObject(),
                settings["testDefaults"]!.AsObject(),
                settings["excerptChars"]!.GetValue<int>()),
            output);
        foreach (var traced in input["events"]!.AsArray())
        {
            trace.Write(Event(traced!) with { Run = traced!["run"]?.GetValue<int>() });
        }
        return output.Text;
    }

    private static TraceEvent Event(JsonNode traced)
    {
        return traced["event"]!.GetValue<string>() switch
        {
            "test-started" => new TestStartedEvent(Int(traced, "runs"), traced["threshold"]!.GetValue<double>(), Long(traced, "deadlineMs")),
            "run-settled" => new RunSettledEvent(Int(traced, "number"), RunRecordJson.Status(traced["status"]), RunRecordJson.Cause(traced["cause"])),
            "test-aborted" => new TestAbortedEvent(RunRecordJson.Cause(traced["cause"])!, Int(traced, "discarded")),
            "test-expired" => new TestExpiredEvent(Long(traced, "deadlineMs"), Int(traced, "discarded")),
            "verdict-reached" => new VerdictReachedEvent(
                new Verdict(Outcomes[Text(traced, "outcome")], Int(traced, "need"), Int(traced, "passed"), Int(traced, "failed"), Int(traced, "errored")),
                Int(traced, "runs")),
            "provider-request" => new ProviderRequestEvent(
                Text(traced, "provider"), Text(traced, "model"), traced["system"]?.GetValue<string>(), traced["meta"]?.AsObject(), Text(traced, "prompt"), Int(traced, "attempt")),
            "provider-response" => new ProviderResponseEvent(Int(traced, "attempt"), Text(traced, "reply")),
            "provider-failure" => new ProviderFailureEvent(
                Int(traced, "attempt"), RunRecordJson.Cause(traced["cause"])!, Nexts[Text(traced, "next")], traced["delayMs"]?.GetValue<double>() ?? 0),
            "judge-reply-rejected" => new JudgeReplyRejectedEvent(Int(traced, "ask"), RunRecordJson.Cause(traced["cause"])!),
            _ => new JudgeRecordedEvent(RunRecordJson.Call(traced["call"])),
        };
    }

    private static string Text(JsonNode traced, string key) => traced[key]!.GetValue<string>();

    private static int Int(JsonNode traced, string key) => traced[key]!.GetValue<int>();

    private static long Long(JsonNode traced, string key) => traced[key]!.GetValue<long>();
}
