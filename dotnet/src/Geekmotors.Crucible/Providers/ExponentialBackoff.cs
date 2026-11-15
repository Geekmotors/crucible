namespace CrucibleAI.Providers;

internal sealed class ExponentialBackoff(TimeProvider time, Func<double> jitter) : IRetryBackoff
{
    private const double BaseDelayMilliseconds = 250;

    public ExponentialBackoff()
        : this(TimeProvider.System, Random.Shared.NextDouble)
    {
    }

    public TimeSpan Delay(int retry) => TimeSpan.FromMilliseconds(BaseDelayMilliseconds * Math.Pow(2, retry - 1) * (1 + jitter()));

    public Task WaitAsync(TimeSpan delay, CancellationToken cancellation) => Task.Delay(delay, time, cancellation);
}
