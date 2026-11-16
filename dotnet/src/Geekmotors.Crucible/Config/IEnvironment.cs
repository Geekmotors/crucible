namespace Geekmotors.Crucible.Config;

internal interface IEnvironment
{
    public string? Get(string name);
}
