namespace CrucibleAI.Tests.Harnesses;

internal sealed class ThrownCapture
{
    private readonly TaskCompletionSource<Exception?> _thrown = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<Exception?> Thrown => _thrown.Task;

    public void Run(Action action)
    {
        try
        {
            action();
            _thrown.SetResult(null);
        }
        catch (Exception failure)
        {
            _thrown.SetResult(failure);
        }
    }

    public async Task RunAsync(Func<Task> action)
    {
        try
        {
            await action();
            _thrown.SetResult(null);
        }
        catch (Exception failure)
        {
            _thrown.SetResult(failure);
            throw;
        }
    }
}
