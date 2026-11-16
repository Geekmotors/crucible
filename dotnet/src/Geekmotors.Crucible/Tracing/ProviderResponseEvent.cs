namespace Geekmotors.Crucible.Tracing;

internal sealed record ProviderResponseEvent(int Attempt, string Reply) : TraceEvent;
