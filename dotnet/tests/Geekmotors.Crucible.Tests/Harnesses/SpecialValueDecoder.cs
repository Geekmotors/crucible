using System.Text.Json;
using System.Text.Json.Nodes;

namespace CrucibleAI.Tests.Harnesses;

internal static class SpecialValueDecoder
{
    private const string Key = "$special";
    private const string Undefined = "undefined";
    private const string Unreadable = "unreadable";

    private static readonly Dictionary<string, double> Numbers = new(StringComparer.Ordinal)
    {
        ["NaN"] = double.NaN,
        ["Infinity"] = double.PositiveInfinity,
        ["-Infinity"] = double.NegativeInfinity,
    };

    public static JsonNode? Decode(JsonNode? node) => node switch
    {
        JsonObject special when Token(special) is { } token => DecodeToken(special, token),
        JsonObject members => DecodeObject(members),
        JsonArray items => new JsonArray(items.Select(Decode).ToArray()),
        _ => node?.DeepClone(),
    };

    public static bool IsUnset(JsonNode? node) => node is JsonObject special && Token(special) == Undefined;

    public static bool IsUnreadable(JsonNode? node) => node is JsonObject special && Token(special) == Unreadable;

    private static JsonObject DecodeObject(JsonObject members)
    {
        var decoded = new JsonObject();
        foreach (var (name, value) in members)
        {
            if (!IsUnset(value))
            {
                decoded[name] = Decode(value);
            }
        }
        return decoded;
    }

    private static JsonNode DecodeToken(JsonObject special, string token)
    {
        if (Numbers.TryGetValue(token, out var number))
        {
            return JsonValue.Create(number);
        }
        if (token is Undefined or Unreadable)
        {
            return special.DeepClone();
        }
        throw new UnknownSpecialTokenException(token);
    }

    private static string? Token(JsonObject candidate)
    {
        if (candidate.Count != 1 || candidate[Key] is not JsonValue value)
        {
            return null;
        }
        return value.GetValueKind() == JsonValueKind.String ? value.GetValue<string>() : null;
    }
}
