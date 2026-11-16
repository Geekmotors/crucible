using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class FixtureOutcome
{
    public static JsonObject Capture(Func<JsonNode?> operation, JsonNode? expect)
    {
        try
        {
            return new JsonObject { ["value"] = operation() };
        }
        catch (CrucibleException error)
        {
            return new JsonObject { ["error"] = Describe(error, expect) };
        }
    }

    private static JsonObject Describe(CrucibleException error, JsonNode? expect)
    {
        var described = new JsonObject { ["kind"] = ErrorKindName.Of(error.Kind) };
        if (error.Retryable is { } retryable)
        {
            described["retryable"] = retryable;
        }
        if (expect?["error"]?["mustNotContain"] is JsonArray secrets)
        {
            var shown = Shown(error);
            described["mustNotContain"] = new JsonArray(secrets
                .Select(secret => secret!.GetValue<string>())
                .Where(secret => !shown.Any(text => text.Contains(secret, StringComparison.Ordinal)))
                .Select(secret => (JsonNode)secret)
                .ToArray());
        }
        return described;
    }

    private static List<string> Shown(Exception error)
    {
        var shown = new List<string>();
        for (Exception? current = error; current is not null; current = current.InnerException)
        {
            shown.Add(current.Message);
            shown.Add(current.ToString());
        }
        return shown;
    }
}
