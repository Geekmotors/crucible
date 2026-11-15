using Geekmotors.Crucible.Runs;

namespace Geekmotors.Crucible.Assertions;

internal sealed record AssertionRun(RunRecord Record, bool Strict, TimeSpan InferenceTimeout, CancellationToken Cancellation);
