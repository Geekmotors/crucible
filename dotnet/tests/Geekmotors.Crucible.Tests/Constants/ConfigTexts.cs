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
}
