using Geekmotors.Crucible.Runs;

namespace Geekmotors.Crucible.Tracing;

internal sealed record ProviderFailureEvent(int Attempt, RunCause Cause, ProviderFailureNext Next, double DelayMs) : TraceEvent;
