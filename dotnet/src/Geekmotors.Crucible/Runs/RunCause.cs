namespace CrucibleAI.Runs;

internal sealed record RunCause(string Name, string Message)
{
    public static RunCause Of(Exception error) => new(error.GetType().Name, error.Message);

    public string Describe() => $"{Name}: {Message}";
}
