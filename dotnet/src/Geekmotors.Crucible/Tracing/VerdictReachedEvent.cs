using Geekmotors.Crucible.Verdicts;

namespace Geekmotors.Crucible.Tracing;

internal sealed record VerdictReachedEvent(Verdict Verdict, int Runs) : TraceEvent;
