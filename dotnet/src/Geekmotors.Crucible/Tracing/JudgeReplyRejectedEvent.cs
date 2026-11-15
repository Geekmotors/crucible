using Geekmotors.Crucible.Runs;

namespace Geekmotors.Crucible.Tracing;

internal sealed record JudgeReplyRejectedEvent(int Ask, RunCause Cause) : TraceEvent;
