namespace CrucibleAI.Providers;

internal interface IJudgeClient
{
    public Task<string> CompleteAsync(JudgeRequest request, IJudgeObserver observer, CancellationToken cancellation);
}
