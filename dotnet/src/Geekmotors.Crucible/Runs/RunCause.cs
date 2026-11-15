namespace Geekmotors.Crucible.Runs;

internal sealed record RunCause(string Name, string Message)
{
    public string Describe() => $"{Name}: {Message}";
}
