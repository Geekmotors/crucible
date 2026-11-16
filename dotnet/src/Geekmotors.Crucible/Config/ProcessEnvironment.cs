namespace Geekmotors.Crucible.Config;

internal sealed class ProcessEnvironment : IEnvironment
{
    public string? Get(string name) => Environment.GetEnvironmentVariable(name);
}
