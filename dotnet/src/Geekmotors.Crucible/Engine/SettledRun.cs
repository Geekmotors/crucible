using CrucibleAI.Runs;

namespace CrucibleAI.Engine;

internal sealed record SettledRun(RunResult Result, Exception? Thrown);
