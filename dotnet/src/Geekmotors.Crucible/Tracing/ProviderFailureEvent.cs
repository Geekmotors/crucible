using CrucibleAI.Runs;

namespace CrucibleAI.Tracing;

internal sealed record ProviderFailureEvent(int Attempt, RunCause Cause, ProviderFailureNext Next, double DelayMs) : TraceEvent;
