namespace CrucibleAI.Config;

internal interface IEnvironment
{
    public string? Get(string name);
}
