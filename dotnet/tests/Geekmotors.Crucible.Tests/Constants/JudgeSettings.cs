namespace Geekmotors.Crucible.Tests.Constants;

internal static class JudgeSettings
{
    public const string KeyVariable = "CRUCIBLE_JUDGE_KEY";
    public const string Key = "sk-spec-sentinel-key";
    public const string OpenRouterConfig = """{ "provider": "openrouter", "model": "judge-model" }""";
    public const string PlainHttpConfig =
        """{ "provider": "openai-compatible", "model": "judge-model", "baseUrl": "http://gateway.example.com/v1" }""";
    public const string HttpsCompatibleConfig =
        """{ "provider": "openai-compatible", "model": "judge-model", "baseUrl": "https://gateway.example.com/v1" }""";
    public const string EscapedPathConfig =
        """{ "provider": "openai-compatible", "model": "judge-model", "baseUrl": "https://gateway.example.com/%41pi/v1" }""";
    public const string EscapedPathEndpoint = "https://gateway.example.com/%41pi/v1/chat/completions";
    public const string HostUnaddressableByUriConfig =
        """{ "provider": "openai-compatible", "model": "judge-model", "baseUrl": "http://gate{way}.example.com/v1" }""";
    public const string UnknownCharset = "x-unknown-charset";
    public const string System = "You are the judge.";
    public const string Prompt = "RESPONSE";
    public const string Reply = """{"verdict":true,"reasoning":"consistent"}""";
}
