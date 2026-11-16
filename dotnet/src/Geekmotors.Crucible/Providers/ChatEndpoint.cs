using CrucibleAI.Config;
using CrucibleAI.Urls;

namespace CrucibleAI.Providers;

internal sealed record ChatEndpoint(string Name, Uri Url, string KeyVariable, bool KeyRequired, HttpUrl? BaseUrl)
{
    public const string DefaultKeyVariable = "CRUCIBLE_JUDGE_KEY";

    private const string CompletionsPath = "/chat/completions";
    private const string OpenRouterBase = "https://openrouter.ai/api/v1";
    private const string OpenAiBase = "https://api.openai.com/v1";

    public static ChatEndpoint For(JudgeConfig config)
    {
        var keyVariable = config.ApiKeyEnv ?? DefaultKeyVariable;
        return config.Provider switch
        {
            ProviderKind.OpenRouter => new("openrouter", Endpoint(OpenRouterBase), keyVariable, true, null),
            ProviderKind.OpenAi => new("openai", Endpoint(OpenAiBase), keyVariable, true, null),
            ProviderKind.OpenAiCompatible => new(
                "openai-compatible",
                Endpoint(config.BaseUrl!.Href.TrimEnd('/')),
                keyVariable,
                config.ApiKeyEnv is not null,
                config.BaseUrl),
            _ => throw new ArgumentOutOfRangeException(nameof(config)),
        };
    }

    private static readonly UriCreationOptions Verbatim = new() { DangerousDisablePathAndQueryCanonicalization = true };

    private static Uri Endpoint(string baseUrl) =>
        Uri.TryCreate(baseUrl + CompletionsPath, Verbatim, out var url)
            ? url
            : throw CrucibleException.Config(
                "\"baseUrl\" in the config file names a host the .NET HTTP client cannot address; use a plain host name or IP address.");
}
