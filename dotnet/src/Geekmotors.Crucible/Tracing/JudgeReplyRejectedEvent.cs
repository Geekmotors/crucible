using CrucibleAI.Runs;

namespace CrucibleAI.Tracing;

internal sealed record JudgeReplyRejectedEvent(int Ask, RunCause Cause) : TraceEvent;
