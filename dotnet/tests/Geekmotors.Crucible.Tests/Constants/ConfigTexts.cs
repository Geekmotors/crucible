namespace CrucibleAI.Tests.Constants;

internal static class ConfigTexts
{
    public const string ExcerptCharsBeyondThirtyTwoBits =
        """{ "provider": "openrouter", "model": "judge-model", "excerptChars": 2147483648 }""";
    public const string TestDefaultsInWrittenOrder =
        """{ "provider": "openrouter", "model": "judge-model", "testDefaults": { "strict": true, "runs": 3 } }""";
    public const string TestDefaultsCompact = """{"strict":true,"runs":3}""";
    public const string WithoutTestDefaults = """{ "provider": "openrouter", "model": "judge-model" }""";
    public const string EmptyObject = "{}";
    public const string ProviderFieldsForAChatClient =
        """{ "provider": "no-such-provider", "model": "judge-model", "meta": { "temperature": 0 }, "testDefaults": { "runs": 3 } }""";
    public const string ChatClientDefaults = """{ "testDefaults": { "runs": 3 }, "excerptChars": 40 }""";
    public const string HalfOfFourRuns = """{ "testDefaults": { "runs": 4, "threshold": 0.5 } }""";
    public const int FailingRuns = 2;
    public const string UnknownFieldForAChatClient = """{ "testDefaults": { "runs": 3 }, "judge": "mine" }""";
    public const string KeyLikeFieldForAChatClient = """{ "apiKey": "sk-not-a-key" }""";
    public const int DefaultExcerptChars = 500;
    public const int ChatClientExcerptChars = 40;
    public const int ChatClientRuns = 3;
    public const string DebugVerbosity = """{ "verbosity": "debug" }""";
}
