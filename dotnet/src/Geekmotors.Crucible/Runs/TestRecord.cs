namespace Geekmotors.Crucible.Runs;

internal sealed record TestRecord(int Runs, double Threshold, TestEnding Ending, IReadOnlyList<RunResult> Results);
