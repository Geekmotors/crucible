using CrucibleAI.Providers;
using CrucibleAI.Tracing;

namespace CrucibleAI.Engine;

internal sealed class RunObserver(DebugTrace trace, int run) : IJudgeObserver
{
    public void Observe(TraceEvent traced) => trace.Write(traced with { Run = run });
}
