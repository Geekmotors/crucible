using System.Globalization;
using Geekmotors.Crucible.Config;
using Geekmotors.Crucible.Judging;
using Geekmotors.Crucible.Runs;
using Geekmotors.Crucible.Text;
using Geekmotors.Crucible.Verdicts;

namespace Geekmotors.Crucible.Tracing;

internal sealed class DebugTrace(TraceSettings settings, ITraceOutput output)
{
    private const string Prefix = "[crucible:debug]";
    private const string BodyIndent = "    ";
    private const int PayloadChars = 2000;
    private const char Ellipsis = (char)0x2026;

    private readonly object _gate = new();
    private bool _announced;

    public void Write(TraceEvent traced)
    {
        if (settings.Verbosity != Verbosity.Debug)
        {
            return;
        }
        var (headline, body) = Describe(traced);
        var tag = traced.Run is { } run ? $"run #{Integer(run)} " : string.Empty;
        var text = $"{Prefix} {tag}{TextFormat.Indented(headline, BodyIndent)}";
        if (body is not null)
        {
            text += "\n" + BodyIndent + TextFormat.Indented(body, BodyIndent);
        }
        lock (_gate)
        {
            if (!_announced)
            {
                _announced = true;
                Deliver(Header());
            }
            Deliver(text + "\n");
        }
    }

    private static (string Headline, string? Body) Describe(TraceEvent traced) => traced switch
    {
        TestStartedEvent started => (
            $"test started: {Integer(started.Runs)} run(s) at threshold {EcmaScriptNumber.Format(started.Threshold)}, deadline {Integer(started.DeadlineMs)} ms",
            null),
        RunSettledEvent settled => (
            $"run #{Integer(settled.Number)} settled {StatusWord(settled.Status)}" + (settled.Cause is { } cause ? ": " + cause.Describe() : string.Empty),
            null),
        TestAbortedEvent aborted => (
            $"test aborted on a fatal error: {aborted.Cause.Describe()}; {Integer(aborted.Discarded)} in-flight run(s) discarded", null),
        TestExpiredEvent expired => (
            $"test deadline of {Integer(expired.DeadlineMs)} ms expired; {Integer(expired.Discarded)} in-flight run(s) discarded", null),
        VerdictReachedEvent reached => (
            $"verdict {ZoneWord(reached.Verdict.Outcome)}: {Integer(reached.Verdict.Passed)} passed, {Integer(reached.Verdict.Failed)} failed, "
            + $"{Integer(reached.Verdict.Errored)} errored of {Integer(reached.Runs)} (need {Integer(reached.Verdict.Need)})",
            null),
        ProviderRequestEvent request => (
            $"provider request attempt {Integer(request.Attempt)} to \"{request.Provider}\": model \"{request.Model}\", "
            + $"system prompt {(request.System is { } system ? Integer(system.Length) + " chars" : "none")}, meta keys [{Keys(request.Meta)}]",
            Payload("prompt", request.Prompt)),
        ProviderResponseEvent response => ($"provider response attempt {Integer(response.Attempt)}", Payload("reply", response.Reply)),
        ProviderFailureEvent failure => (
            $"provider attempt {Integer(failure.Attempt)} failed ({NextWords(failure)}): {failure.Cause.Describe()}", null),
        JudgeReplyRejectedEvent rejected => ($"judge reply {Integer(rejected.Ask)} unusable: {rejected.Cause.Describe()}", null),
        JudgeRecordedEvent recorded => Recorded(recorded.Call),
        _ => throw new ArgumentException("Unknown trace event.", nameof(traced)),
    };

    private static (string Headline, string? Body) Recorded(JudgeCall call) => call switch
    {
        VerdictCall verdict => (
            $"judge verdict {(verdict.Verdict ? "true" : "false")} for coherent \"{verdict.Claim}\"", Payload("reasoning", verdict.Reasoning)),
        ExtractionCall extraction => (
            $"judge value {EcmaScriptJson.Value(extraction.Value)} for extract \"{extraction.Description}\" as "
            + $"{ExtractWords.Of(extraction.Type)} ({ExtractWords.Of(extraction.Mode)})",
            Payload("reasoning", extraction.Reasoning)),
        ErroredCall error => ($"judge call errored after {Integer(error.Attempts)} attempt(s): {error.Cause.Describe()}", null),
        _ => throw new ArgumentException("Unknown judge call.", nameof(call)),
    };

    private string Header() =>
        $"{Prefix} engine init: verbosity {VerbosityWord(settings.Verbosity)}, trace written to stdout\n"
        + $"{Prefix} config resolved: provider \"{settings.Provider}\", model \"{settings.Model}\", meta keys [{Keys(settings.Meta)}] (values not shown), "
        + $"testDefaults {EcmaScriptJson.Node(settings.TestDefaults)}, excerptChars {Integer(settings.ExcerptChars)}\n";

    private static string Payload(string label, string value)
    {
        if (value.Length <= PayloadChars)
        {
            return $"{label} ({Integer(value.Length)} chars): {value}";
        }
        var cut = TextFormat.Cut(value, PayloadChars);
        return $"{label} (first {Integer(cut.Length)} of {Integer(value.Length)} chars): {cut}{Ellipsis}";
    }

    private static string NextWords(ProviderFailureEvent failure) => failure.Next switch
    {
        ProviderFailureNext.Retry => $"retryable, retrying in {EcmaScriptNumber.Format(EcmaScriptMath.Round(failure.DelayMs))} ms",
        ProviderFailureNext.Fatal => "fatal, not retried",
        ProviderFailureNext.Exhausted => "retryable, but no attempts left",
        _ => "the call was aborted",
    };

    private static string Keys(System.Text.Json.Nodes.JsonObject? meta) => string.Join(", ", meta?.Select(member => member.Key) ?? []);

    private static string StatusWord(RunStatus status) => status switch
    {
        RunStatus.Passed => "passed",
        RunStatus.Failed => "failed",
        RunStatus.Errored => "errored",
        _ => "discarded",
    };

    private static string ZoneWord(VerdictOutcome outcome) => outcome switch
    {
        VerdictOutcome.Passed => "pass",
        VerdictOutcome.Failed => "fail",
        _ => "errored",
    };

    private static string VerbosityWord(Verbosity verbosity) => verbosity switch
    {
        Verbosity.Full => "full",
        Verbosity.Debug => "debug",
        _ => "default",
    };

    private void Deliver(string text)
    {
        try
        {
            output.Write(text);
        }
        catch (Exception failure) when (failure is not OutOfMemoryException)
        {
        }
    }

    private static string Integer(long value) => value.ToString(CultureInfo.InvariantCulture);
}
