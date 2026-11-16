namespace CrucibleAI.Runs;

internal sealed record AbortedEnding(int FatalRun) : TestEnding;
