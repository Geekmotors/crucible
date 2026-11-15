namespace Geekmotors.Crucible.Tracing;

internal sealed record TestStartedEvent(int Runs, double Threshold, long DeadlineMs) : TraceEvent;
