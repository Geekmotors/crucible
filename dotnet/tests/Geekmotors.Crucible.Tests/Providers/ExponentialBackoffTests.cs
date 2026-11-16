using CrucibleAI.Providers;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;

namespace CrucibleAI.Tests.Providers;

public sealed class ExponentialBackoffTests
{
    private readonly ManualTimeProvider _clock = new();

    [Fact]
    public void TheFirstRetryWaitsTheBaseDelay()
    {
        var backoff = new ExponentialBackoff(_clock, () => BackoffTimes.NoJitter);
        var wait = backoff.WaitAsync(backoff.Delay(BackoffTimes.FirstRetry), CancellationToken.None);

        _clock.Advance(BackoffTimes.FirstRetryWithoutJitter - BackoffTimes.Tick);
        var early = wait.IsCompleted;
        _clock.Advance(BackoffTimes.Tick);

        Assert.Equal((false, true), (early, wait.IsCompleted));
    }

    [Fact]
    public void TheSecondRetryWaitsTwiceTheBaseDelay()
    {
        var backoff = new ExponentialBackoff(_clock, () => BackoffTimes.NoJitter);
        var wait = backoff.WaitAsync(backoff.Delay(BackoffTimes.SecondRetry), CancellationToken.None);

        _clock.Advance(BackoffTimes.SecondRetryWithoutJitter - BackoffTimes.Tick);
        var early = wait.IsCompleted;
        _clock.Advance(BackoffTimes.Tick);

        Assert.Equal((false, true), (early, wait.IsCompleted));
    }

    [Fact]
    public void JitterStretchesTheWaitUpToDouble()
    {
        var backoff = new ExponentialBackoff(_clock, () => BackoffTimes.FullJitter);
        var wait = backoff.WaitAsync(backoff.Delay(BackoffTimes.SecondRetry), CancellationToken.None);

        _clock.Advance(BackoffTimes.SecondRetryWithFullJitter - BackoffTimes.Tick);
        var early = wait.IsCompleted;
        _clock.Advance(BackoffTimes.Tick);

        Assert.Equal((false, true), (early, wait.IsCompleted));
    }
}
