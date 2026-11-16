namespace CrucibleAI.LiveTests;

public static class LiveKey
{
    public static bool IsSet => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(LiveTexts.KeyVariable));
}
