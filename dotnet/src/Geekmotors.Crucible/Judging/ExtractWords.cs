namespace CrucibleAI.Judging;

internal static class ExtractWords
{
    public static string Of(ExtractType type) => type switch
    {
        ExtractType.Number => "number",
        ExtractType.Boolean => "boolean",
        _ => "string",
    };

    public static string Of(ExtractMode mode) => mode == ExtractMode.Strict ? "strict" : "lenient";
}
