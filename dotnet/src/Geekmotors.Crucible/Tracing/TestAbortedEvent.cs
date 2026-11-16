using Geekmotors.Crucible.Runs;

namespace Geekmotors.Crucible.Tracing;

internal sealed record TestAbortedEvent(RunCause Cause, int Discarded) : TraceEvent;
