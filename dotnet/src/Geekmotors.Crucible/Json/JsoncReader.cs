using System.Text.Json.Nodes;

namespace CrucibleAI.Json;

internal sealed class JsoncReader
{
    public JsonNode? Read(string text)
    {
        try
        {
            return JsonTextParser.Parse(text, JsonDialect.Jsonc);
        }
        catch (JsonSyntaxException failure)
        {
            throw CrucibleException.Config(
                $"The config file is not valid JSON (line {failure.Line}). Fix the syntax; comments and trailing commas are allowed.");
        }
    }
}
