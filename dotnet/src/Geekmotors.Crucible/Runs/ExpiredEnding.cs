namespace CrucibleAI.Runs;

internal sealed record ExpiredEnding(long DeadlineMs) : TestEnding;
