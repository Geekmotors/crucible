using CrucibleAI.Providers;

namespace CrucibleAI.Tests.Fakes;

internal sealed class HangingJudgeClient : IJudgeClient
{
    private readonly TaskCompletionSource _reached = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Reached => _reached.Task;

    public async Task<string> CompleteAsync(JudgeRequest request, IJudgeObserver observer, CancellationToken cancellation)
    {
        _reached.TrySetResult();
        await Task.Delay(Timeout.Infinite, cancellation);
        throw new OperationCanceledException(cancellation);
    }
}
