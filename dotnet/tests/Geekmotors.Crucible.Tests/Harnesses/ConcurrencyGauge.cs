namespace CrucibleAI.Tests.Harnesses;

internal sealed class ConcurrencyGauge
{
    private int _active;
    private int _most;

    public int Most => Volatile.Read(ref _most);

    public void Enter()
    {
        var now = Interlocked.Increment(ref _active);
        var seen = Volatile.Read(ref _most);
        while (now > seen && Interlocked.CompareExchange(ref _most, now, seen) != seen)
        {
            seen = Volatile.Read(ref _most);
        }
    }

    public void Leave() => Interlocked.Decrement(ref _active);
}
