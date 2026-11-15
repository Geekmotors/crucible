using CrucibleAI.Assertions;
using CrucibleAI.Config;
using CrucibleAI.Judging;
using CrucibleAI.Providers;
using CrucibleAI.Reports;
using CrucibleAI.Tracing;

namespace CrucibleAI.Engine;

internal sealed class EngineSession
{
    private readonly ITraceOutput _output;
    private readonly Verbosity _verbosity;

    public EngineSession(JudgeConfig config, IJudgeClient judge, TimeProvider time, ITraceOutput output)
    {
        var exchange = new JudgeExchange(judge, time);
        _output = output;
        _verbosity = config.Verbosity;
        TestDefaults = config.TestDefaults;
        Coherent = new CoherentAssertion(exchange);
        Extract = new ExtractAssertion(exchange);
        Reports = new ReportRenderer(new ReportSettings(config.ExcerptChars, config.Verbosity));
        Trace = new DebugTrace(
            new TraceSettings(config.Verbosity, ChatEndpoint.For(config).Name, config.Model, config.Meta, config.TestDefaultsNode, config.ExcerptChars),
            output);
    }

    public RunOptions TestDefaults { get; }

    public CoherentAssertion Coherent { get; }

    public ExtractAssertion Extract { get; }

    public ReportRenderer Reports { get; }

    public DebugTrace Trace { get; }

    public static EngineSession FromConfigFile(IJudgeClient? judge)
    {
        var config = new ConfigLoader().Load();
        return new EngineSession(config, judge ?? new ChatCompletionsClient(config), TimeProvider.System, new StandardOutputTraceOutput());
    }

    public void WritePassingReport(Func<string> report)
    {
        if (_verbosity == Verbosity.Default)
        {
            return;
        }
        try
        {
            _output.Write(report() + "\n");
        }
        catch (Exception failure) when (failure is not OutOfMemoryException)
        {
        }
    }
}
