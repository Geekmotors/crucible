using Geekmotors.Crucible.Runs;
using Geekmotors.Crucible.Verdicts;

namespace Geekmotors.Crucible.Tracing;

internal sealed record RunSettledEvent(int Number, RunStatus Status, RunCause? Cause) : TraceEvent;
