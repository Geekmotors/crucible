using CrucibleAI.Runs;
using CrucibleAI.Verdicts;

namespace CrucibleAI.Tracing;

internal sealed record RunSettledEvent(int Number, RunStatus Status, RunCause? Cause) : TraceEvent;
