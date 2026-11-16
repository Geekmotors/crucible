namespace Geekmotors.Crucible.Tracing;

internal sealed record TestExpiredEvent(long DeadlineMs, int Discarded) : TraceEvent;
