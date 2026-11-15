using CrucibleAI.Verdicts;

namespace CrucibleAI.Tracing;

internal sealed record VerdictReachedEvent(Verdict Verdict, int Runs) : TraceEvent;
