namespace CrucibleAI.Engine;

internal sealed record RunBody(Func<CrucibleContext, Task> Invoke, Func<ValueTask> TeardownAsync)
{
    public static RunBody Of(Func<CrucibleContext, Task> body) => new(body, () => ValueTask.CompletedTask);
}
