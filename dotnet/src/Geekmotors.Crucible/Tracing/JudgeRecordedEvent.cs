using Geekmotors.Crucible.Runs;

namespace Geekmotors.Crucible.Tracing;

internal sealed record JudgeRecordedEvent(JudgeCall Call) : TraceEvent;
