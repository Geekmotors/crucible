using System.Text.Json.Nodes;
using Geekmotors.Crucible.Config;
using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Tests.Fakes;
using Geekmotors.Crucible.Tracing;

namespace Geekmotors.Crucible.Tests.Tracing;

public sealed class DebugTraceTests
{
    public static TheoryData<Exception> OutputFailures => new()
    {
        new IOException(),
        new ObjectDisposedException(nameof(ITraceOutput)),
        new InvalidOperationException(),
        new UnauthorizedAccessException(),
        new NotSupportedException(),
    };

    [Theory]
    [MemberData(nameof(OutputFailures))]
    public void ABrokenOutputNeverFailsTheCaller(Exception failure)
    {
        var trace = new DebugTrace(Settings(), new BrokenTraceOutput(failure));

        var thrown = Record.Exception(() => trace.Write(new ProviderResponseEvent(1, TraceTexts.Reply)));

        Assert.Null(thrown);
    }

    [Fact]
    public void TheHeaderIsWrittenOnceAcrossConcurrentEvents()
    {
        var output = new RecordingTraceOutput();
        var trace = new DebugTrace(Settings(), output);

        Parallel.For(0, TraceTexts.ConcurrentEvents, attempt => trace.Write(new ProviderResponseEvent(attempt, TraceTexts.Reply)));

        Assert.Single(output.Text.Split('\n'), line => line.Contains(TraceTexts.EngineInit, StringComparison.Ordinal));
    }

    private static TraceSettings Settings() =>
        new(Verbosity.Debug, TraceTexts.Provider, TraceTexts.Model, null, new JsonObject(), TraceTexts.ExcerptChars);
}
