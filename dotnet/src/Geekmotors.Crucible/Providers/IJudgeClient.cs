namespace Geekmotors.Crucible.Providers;

internal interface IJudgeClient
{
    public Task<string> CompleteAsync(JudgeRequest request, CancellationToken cancellation);
}
