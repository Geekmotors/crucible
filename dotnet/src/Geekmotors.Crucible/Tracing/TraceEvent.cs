namespace CrucibleAI.Tracing;

internal abstract record TraceEvent
{
    public int? Run { get; init; }
}
