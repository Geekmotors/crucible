using System.Globalization;
using System.Text.Json.Nodes;
using Geekmotors.Crucible.Json;
using Geekmotors.Crucible.Providers;

namespace Geekmotors.Crucible.Judging;

internal sealed class JudgeExchange(IJudgeClient client, TimeProvider time)
{
    private const int MaxAsks = 2;

    public async Task<T> AskAsync<T>(JudgeRequest request, Func<JsonNode?, T?> read, TimeSpan inferenceTimeout, CancellationToken cancellation)
        where T : class
    {
        for (var ask = 1; ask <= MaxAsks; ask++)
        {
            var reply = await CompleteBeforeDeadlineAsync(request, inferenceTimeout, cancellation).ConfigureAwait(false);
            if (Usable(reply, read) is { } result)
            {
                return result;
            }
        }
        throw CrucibleException.Infra($"The judge reply was not the expected JSON, {MaxAsks} times in a row.", true);
    }

    private async Task<string> CompleteBeforeDeadlineAsync(JudgeRequest request, TimeSpan inferenceTimeout, CancellationToken cancellation)
    {
        using var deadline = new CancellationTokenSource(inferenceTimeout, time);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, deadline.Token);
        try
        {
            return await client.CompleteAsync(request, linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException timedOut) when (deadline.IsCancellationRequested && !cancellation.IsCancellationRequested)
        {
            throw CrucibleException.Infra(
                string.Create(CultureInfo.InvariantCulture, $"The judge call did not finish within {inferenceTimeout.TotalMilliseconds} ms."),
                true,
                timedOut);
        }
    }

    private static T? Usable<T>(string reply, Func<JsonNode?, T?> read)
        where T : class
    {
        try
        {
            return read(JsonTextParser.Parse(JudgeReplyText.Unwrap(reply), JsonDialect.Strict));
        }
        catch (JsonSyntaxException)
        {
            return null;
        }
    }
}
