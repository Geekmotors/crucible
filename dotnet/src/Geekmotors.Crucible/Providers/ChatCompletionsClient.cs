using System.Net.Http.Headers;
using Geekmotors.Crucible.Config;

namespace Geekmotors.Crucible.Providers;

internal sealed class ChatCompletionsClient(JudgeConfig config, HttpClient http, IEnvironment environment, IRetryBackoff backoff, IWarningSink warnings)
    : IJudgeClient
{
    private const int MaxAttempts = 3;
    private const string JsonMediaType = "application/json";
    private const string AuthorizationHeader = "Authorization";

    private static readonly HttpClient SharedHttp = new(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) })
    {
        Timeout = Timeout.InfiniteTimeSpan,
    };

    private readonly ChatEndpoint _endpoint = ChatEndpoint.For(config);
    private readonly JudgeKeyReader _keys = new(environment);
    private readonly ChatResponseReader _responses = new();
    private readonly PlainHttpRule _plainHttp = new();
    private int _warnedPlainHttp;

    public ChatCompletionsClient(JudgeConfig config)
        : this(config, SharedHttp, new ProcessEnvironment(), new ExponentialBackoff(), new StandardErrorWarningSink())
    {
    }

    public async Task<string> CompleteAsync(JudgeRequest request, CancellationToken cancellation)
    {
        var key = _keys.Read(_endpoint);
        var body = ChatRequestBody.Serialize(config.Model, config.Meta, request);
        WarnIfKeyTravelsInPlainText(key);
        Exception? lastFailure = null;
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            cancellation.ThrowIfCancellationRequested();
            try
            {
                using var message = Message(key, body);
                using var response = await http.SendAsync(message, cancellation).ConfigureAwait(false);
                return await _responses.ReadAsync(response, cancellation).ConfigureAwait(false);
            }
            catch (ProviderAttemptException failure) when (failure.Fatal)
            {
                throw CrucibleException.Infra($"The \"{_endpoint.Name}\" judge refused the request: {failure.Message}", false, failure);
            }
            catch (Exception failure) when (failure is ProviderAttemptException or HttpRequestException or IOException)
            {
                cancellation.ThrowIfCancellationRequested();
                lastFailure = failure;
            }
            if (attempt < MaxAttempts)
            {
                await backoff.WaitAsync(attempt, cancellation).ConfigureAwait(false);
            }
        }
        throw CrucibleException.Infra($"The \"{_endpoint.Name}\" judge kept failing after {MaxAttempts} attempts.", true, lastFailure);
    }

    private void WarnIfKeyTravelsInPlainText(string? key)
    {
        if (_endpoint.BaseUrl is { } baseUrl && _plainHttp.Warns(baseUrl, key is not null)
            && Interlocked.Exchange(ref _warnedPlainHttp, 1) == 0)
        {
            warnings.Write(
                $"Crucible: the judge key is sent unencrypted to {baseUrl.Host} over plain http. "
                + "Use https, or a loopback address for a local judge.");
        }
    }

    private HttpRequestMessage Message(string? key, byte[] body)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, _endpoint.Url)
        {
            Content = new ByteArrayContent(body) { Headers = { ContentType = new MediaTypeHeaderValue(JsonMediaType) } },
        };
        if (key is not null)
        {
            message.Headers.TryAddWithoutValidation(AuthorizationHeader, $"Bearer {key}");
        }
        return message;
    }
}
