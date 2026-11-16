using CrucibleAI.Providers;
using CrucibleAI.Runs;
using CrucibleAI.Tracing;

namespace CrucibleAI.Assertions;

internal sealed record AssertionRun(RunRecord Record, bool Strict, TimeSpan InferenceTimeout, IJudgeObserver Observer, CancellationToken Cancellation)
{
    public async Task<T> JudgeAsync<T>(Func<IJudgeObserver, Task<T>> ask)
    {
        var attempts = new AttemptCounter(Observer);
        try
        {
            return await ask(attempts).ConfigureAwait(false);
        }
        catch (Exception failure) when (failure is not CrucibleException { Kind: CrucibleErrorKind.Config })
        {
            Recorded(new ErroredCall(RunCause.Of(failure), attempts.Attempts));
            throw;
        }
    }

    public void Recorded(JudgeCall call)
    {
        Record.Record(call);
        Observer.Observe(new JudgeRecordedEvent(call));
    }
}
