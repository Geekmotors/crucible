namespace CrucibleAI.Json;

internal sealed class JsonSyntaxException(int line) : Exception($"Invalid JSON at line {line}.")
{
    public int Line { get; } = line;
}
