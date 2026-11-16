using CrucibleAI.Runs;

namespace CrucibleAI.Tracing;

internal sealed record JudgeRecordedEvent(JudgeCall Call) : TraceEvent;
