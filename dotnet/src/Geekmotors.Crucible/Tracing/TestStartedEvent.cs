namespace CrucibleAI.Tracing;

internal sealed record TestStartedEvent(int Runs, double Threshold, long DeadlineMs) : TraceEvent;
