using CrucibleAI.Providers;

namespace CrucibleAI.Tests.Fakes;

internal sealed class ImmediateBackoff : IRetryBackoff
{
    public TimeSpan Delay(int retry) => TimeSpan.Zero;

    public Task WaitAsync(TimeSpan delay, CancellationToken cancellation) => Task.CompletedTask;
}
