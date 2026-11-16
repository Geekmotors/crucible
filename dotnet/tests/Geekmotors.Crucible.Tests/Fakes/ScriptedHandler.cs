namespace Geekmotors.Crucible.Tests.Fakes;

internal sealed class ScriptedHandler(IEnumerable<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>> answers)
    : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>> _answers = new(answers);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        _answers.TryDequeue(out var answer) ? answer(request, cancellationToken) : throw new ScriptExhaustedException();
}
