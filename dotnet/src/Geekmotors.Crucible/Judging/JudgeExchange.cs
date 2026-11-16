using System.Globalization;
using System.Text.Json.Nodes;
using CrucibleAI.Json;
using CrucibleAI.Providers;
using CrucibleAI.Runs;
using CrucibleAI.Tracing;

namespace CrucibleAI.Judging;

internal sealed class JudgeExchange(IJudgeClient client, TimeProvider time)
{
    private const int MaxAsks = 2;

    public async Task<T> AskAsync<T>(
        JudgeRequest request, Func<JsonNode?, T?> read, TimeSpan inferenceTimeout, IJudgeObserver observer, CancellationToken cancellation)
        where T : class
    {
        CrucibleException? rejection = null;
        for (var ask = 1; ask <= MaxAsks; ask++)
        {
            var reply = await CompleteBeforeDeadlineAsync(request, inferenceTimeout, observer, cancellation).ConfigureAwait(false);
            var (result, problem) = Usable(reply, read);
            if (result is not null)
            {
                return result;
            }
            rejection = CrucibleException.Infra("The judge reply was not the expected JSON.", true, problem);
            observer.Observe(new JudgeReplyRejectedEvent(ask, RunCause.Of(rejection)));
        }
        throw rejection!;
    }

    private async Task<string> CompleteBeforeDeadlineAsync(
        JudgeRequest request, TimeSpan inferenceTimeout, IJudgeObserver observer, CancellationToken cancellation)
    {
        using var deadline = new CancellationTokenSource(inferenceTimeout, time);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, deadline.Token);
        try
        {
            return await client.CompleteAsync(request, observer, linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException timedOut) when (deadline.IsCancellationRequested && !cancellation.IsCancellationRequested)
        {
            throw CrucibleException.Infra(
                string.Create(CultureInfo.InvariantCulture, $"The judge call did not finish within {inferenceTimeout.TotalMilliseconds} ms."),
                true,
                timedOut);
        }
    }

    private static (T? Result, Exception? Problem) Usable<T>(string reply, Func<JsonNode?, T?> read)
        where T : class
    {
        try
        {
            return (read(JsonTextParser.Parse(JudgeReplyText.Unwrap(reply), JsonDialect.JudgeReply)), null);
        }
        catch (JsonSyntaxException problem)
        {
            return (null, problem);
        }
    }
}
