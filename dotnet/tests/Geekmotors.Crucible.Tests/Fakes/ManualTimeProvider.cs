namespace Geekmotors.Crucible.Tests.Fakes;

internal sealed class ManualTimeProvider : TimeProvider
{
    private readonly List<ManualTimer> _timers = [];
    private DateTimeOffset _now = DateTimeOffset.UnixEpoch;

    public override DateTimeOffset GetUtcNow() => _now;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new ManualTimer(this, callback, state);
        timer.Change(dueTime, period);
        lock (_timers)
        {
            _timers.Add(timer);
        }
        return timer;
    }

    public void Advance(TimeSpan by)
    {
        _now += by;
        while (NextDue() is { } timer)
        {
            timer.Fire();
        }
    }

    private ManualTimer? NextDue()
    {
        lock (_timers)
        {
            return _timers.Where(timer => timer.Due <= _now).OrderBy(timer => timer.Due).FirstOrDefault();
        }
    }
}
