namespace CrucibleAI.Providers;

internal interface IRetryBackoff
{
    public TimeSpan Delay(int retry);

    public Task WaitAsync(TimeSpan delay, CancellationToken cancellation);
}
