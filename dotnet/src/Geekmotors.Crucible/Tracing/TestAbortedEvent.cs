using CrucibleAI.Runs;

namespace CrucibleAI.Tracing;

internal sealed record TestAbortedEvent(RunCause Cause, int Discarded) : TraceEvent;
