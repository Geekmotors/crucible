using CrucibleAI.Config;
using CrucibleAI.Text;

namespace CrucibleAI.Providers;

internal sealed class JudgeKeyReader(IEnvironment environment)
{
    public string? Read(ChatEndpoint endpoint)
    {
        var key = EcmaScriptTrim.Trim(environment.Get(endpoint.KeyVariable) ?? string.Empty);
        if (key.Length == 0)
        {
            return endpoint.KeyRequired
                ? throw CrucibleException.Config(
                    $"Set the {endpoint.KeyVariable} environment variable to the API key for the \"{endpoint.Name}\" judge.")
                : null;
        }
        if (!key.All(unit => unit is >= '!' and <= '~'))
        {
            throw CrucibleException.Config(
                $"The {endpoint.KeyVariable} environment variable holds characters that cannot appear in an API key.");
        }
        return key;
    }
}
