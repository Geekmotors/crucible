using Geekmotors.Crucible.Verdicts;

namespace Geekmotors.Crucible.Runs;

internal sealed record RunResult(RunStatus Status, RunCause? Cause, IReadOnlyList<JudgeCall> Calls);
