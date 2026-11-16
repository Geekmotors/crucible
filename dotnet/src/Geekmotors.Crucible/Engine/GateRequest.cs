namespace CrucibleAI.Engine;

internal sealed record GateRequest(
    IReadOnlyList<RunOptions> Groups,
    RunOptions Declared,
    Func<int, Func<CrucibleContext, Task>> BodyFor,
    bool Sequential);
