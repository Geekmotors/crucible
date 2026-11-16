namespace Geekmotors.Crucible.Tests.Fakes;

internal sealed class ManualTimer(ManualTimeProvider clock, TimerCallback callback, object? state) : ITimer
{
    private TimeSpan _period = Timeout.InfiniteTimeSpan;

    public DateTimeOffset? Due { get; private set; }

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        Due = dueTime == Timeout.InfiniteTimeSpan ? null : clock.GetUtcNow() + dueTime;
        _period = period;
        return true;
    }

    public void Fire()
    {
        Due = _period > TimeSpan.Zero && _period != Timeout.InfiniteTimeSpan ? Due + _period : null;
        callback(state);
    }

    public void Dispose() => Due = null;

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
