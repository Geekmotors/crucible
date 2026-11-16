namespace CrucibleAI.Tests.Harnesses;

internal sealed class BlockingCaller(SynchronizationContext context)
{
    public Task<Exception?> BlockOnAsync(Func<Task> call)
    {
        var outcome = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var caller = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                call().GetAwaiter().GetResult();
                outcome.SetResult(null);
            }
            catch (Exception failure)
            {
                outcome.SetResult(failure);
            }
        })
        {
            IsBackground = true,
        };
        caller.Start();
        return outcome.Task;
    }
}
