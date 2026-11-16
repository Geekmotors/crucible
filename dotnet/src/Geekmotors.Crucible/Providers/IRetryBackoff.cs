namespace Geekmotors.Crucible.Providers;

internal interface IRetryBackoff
{
    public Task WaitAsync(int retry, CancellationToken cancellation);
}
