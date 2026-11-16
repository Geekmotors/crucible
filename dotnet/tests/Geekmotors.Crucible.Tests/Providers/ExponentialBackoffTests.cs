using Geekmotors.Crucible.Providers;
using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Tests.Fakes;

namespace Geekmotors.Crucible.Tests.Providers;

public sealed class ExponentialBackoffTests
{
    private readonly ManualTimeProvider _clock = new();

    [Fact]
    public void TheFirstRetryWaitsTheBaseDelay()
    {
        var wait = new ExponentialBackoff(_clock, () => BackoffTimes.NoJitter).WaitAsync(BackoffTimes.FirstRetry, CancellationToken.None);

        _clock.Advance(BackoffTimes.FirstRetryWithoutJitter - BackoffTimes.Tick);
        var early = wait.IsCompleted;
        _clock.Advance(BackoffTimes.Tick);

        Assert.Equal((false, true), (early, wait.IsCompleted));
    }

    [Fact]
    public void TheSecondRetryWaitsTwiceTheBaseDelay()
    {
        var wait = new ExponentialBackoff(_clock, () => BackoffTimes.NoJitter).WaitAsync(BackoffTimes.SecondRetry, CancellationToken.None);

        _clock.Advance(BackoffTimes.SecondRetryWithoutJitter - BackoffTimes.Tick);
        var early = wait.IsCompleted;
        _clock.Advance(BackoffTimes.Tick);

        Assert.Equal((false, true), (early, wait.IsCompleted));
    }

    [Fact]
    public void JitterStretchesTheWaitUpToDouble()
    {
        var wait = new ExponentialBackoff(_clock, () => BackoffTimes.FullJitter).WaitAsync(BackoffTimes.SecondRetry, CancellationToken.None);

        _clock.Advance(BackoffTimes.SecondRetryWithFullJitter - BackoffTimes.Tick);
        var early = wait.IsCompleted;
        _clock.Advance(BackoffTimes.Tick);

        Assert.Equal((false, true), (early, wait.IsCompleted));
    }
}
