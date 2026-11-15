namespace CrucibleAI.Xunit;

internal sealed record GatePlan(IReadOnlyList<RunOptions> Groups, RunOptions Declared, bool Sequential);
