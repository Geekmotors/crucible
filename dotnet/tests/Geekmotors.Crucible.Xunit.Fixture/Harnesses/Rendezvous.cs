using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Harnesses;

internal sealed class Rendezvous(int parties)
{
    private readonly TaskCompletionSource _everyone = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _arrived;

    public Task ArriveAsync()
    {
        if (Interlocked.Increment(ref _arrived) == parties)
        {
            _everyone.SetResult();
        }
        return _everyone.Task.WaitAsync(FixtureRuns.MeetingTimeout);
    }
}
