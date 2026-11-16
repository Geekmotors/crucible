namespace Geekmotors.Crucible.Providers;

internal sealed class ExponentialBackoff(TimeProvider time, Func<double> jitter) : IRetryBackoff
{
    private const double BaseDelayMilliseconds = 250;

    public ExponentialBackoff()
        : this(TimeProvider.System, Random.Shared.NextDouble)
    {
    }

    public Task WaitAsync(int retry, CancellationToken cancellation)
    {
        var delay = BaseDelayMilliseconds * Math.Pow(2, retry - 1) * (1 + jitter());
        return Task.Delay(TimeSpan.FromMilliseconds(delay), time, cancellation);
    }
}
