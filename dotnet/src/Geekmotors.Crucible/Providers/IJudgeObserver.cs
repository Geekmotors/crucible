using CrucibleAI.Tracing;

namespace CrucibleAI.Providers;

internal interface IJudgeObserver
{
    public void Observe(TraceEvent traced);
}
