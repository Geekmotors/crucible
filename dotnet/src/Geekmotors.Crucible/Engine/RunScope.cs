namespace CrucibleAI.Engine;

internal sealed class RunScope
{
    private static readonly AsyncLocal<RunState?> Ambient = new();

    public RunState Current => Ambient.Value is { IsOpen: true } run
        ? run
        : throw CrucibleException.Usage("This Crucible call only works inside a running Crucible.Gate body (or a Crucible test).");

    public void Enter(RunState? run) => Ambient.Value = run;
}
