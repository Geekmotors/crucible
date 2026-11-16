namespace CrucibleAI.Runs;

internal sealed record ErroredCall(RunCause Cause, int Attempts) : JudgeCall;
