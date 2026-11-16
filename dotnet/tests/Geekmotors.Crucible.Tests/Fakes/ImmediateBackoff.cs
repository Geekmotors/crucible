using Geekmotors.Crucible.Providers;

namespace Geekmotors.Crucible.Tests.Fakes;

internal sealed class ImmediateBackoff : IRetryBackoff
{
    public Task WaitAsync(int retry, CancellationToken cancellation) => Task.CompletedTask;
}
