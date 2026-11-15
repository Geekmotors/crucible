using System.Text.Json;
using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class JsonEquivalence
{
    private static readonly JsonSerializerOptions Printing = new()
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
    };

    public static bool AreEquivalent(JsonNode? expected, JsonNode? actual) => (expected, actual) switch
    {
        (null, null) => true,
        (JsonObject left, JsonObject right) => SameMembers(left, right),
        (JsonArray left, JsonArray right) => left.Count == right.Count
            && left.Zip(right).All(pair => AreEquivalent(pair.First, pair.Second)),
        (JsonValue left, JsonValue right) => SameValue(left, right),
        _ => false,
    };

    public static string Print(JsonNode? node) => node?.ToJsonString(Printing) ?? "null";

    private static bool SameMembers(JsonObject left, JsonObject right) =>
        left.Count == right.Count
        && left.All(member => right.TryGetPropertyValue(member.Key, out var other) && AreEquivalent(member.Value, other));

    private static bool SameValue(JsonValue left, JsonValue right)
    {
        if (Number(left) is { } leftNumber)
        {
            return Number(right) is { } rightNumber && leftNumber.Equals(rightNumber);
        }
        if (Number(right) is not null)
        {
            return false;
        }
        return JsonNode.DeepEquals(left, right);
    }

    private static double? Number(JsonValue value)
    {
        if (value.TryGetValue<JsonElement>(out var element))
        {
            return element.ValueKind == JsonValueKind.Number ? element.GetDouble() : null;
        }
        if (value.TryGetValue<double>(out var real))
        {
            return real;
        }
        if (value.TryGetValue<long>(out var whole))
        {
            return whole;
        }
        return value.TryGetValue<int>(out var small) ? small : null;
    }
}
