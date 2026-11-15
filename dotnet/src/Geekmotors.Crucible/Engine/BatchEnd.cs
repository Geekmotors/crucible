using CrucibleAI.Runs;

namespace CrucibleAI.Engine;

internal sealed record BatchEnd(TestEnding Ending, IReadOnlyList<SettledRun> Runs, Exception? Cause, Exception? Rethrow);
