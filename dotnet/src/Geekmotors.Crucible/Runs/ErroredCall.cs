namespace Geekmotors.Crucible.Runs;

internal sealed record ErroredCall(RunCause Cause, int Attempts) : JudgeCall;
