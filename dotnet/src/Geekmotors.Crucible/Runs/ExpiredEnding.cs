namespace Geekmotors.Crucible.Runs;

internal sealed record ExpiredEnding(long DeadlineMs) : TestEnding;
