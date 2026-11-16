using System.Text.Json;
using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class CallArguments
{
    private static readonly Dictionary<string, Type> Types = new(StringComparer.Ordinal)
    {
        ["string"] = typeof(string),
        ["number"] = typeof(double),
        ["boolean"] = typeof(bool),
        ["date"] = typeof(DateTime),
    };

    public static string? Text(JsonNode? argument) => argument switch
    {
        null => null,
        _ when SpecialValueDecoder.IsUnset(argument) => null,
        JsonValue value when value.GetValueKind() == JsonValueKind.String => value.GetValue<string>(),
        _ => throw new InexpressibleInputException(argument.ToJsonString()),
    };

    public static IReadOnlyList<string?> Facts(JsonNode? argument) => argument switch
    {
        JsonArray entries => entries.Select(Text).ToArray(),
        _ => [Text(argument)],
    };

    public static IReadOnlyList<string>? Strings(JsonNode? list) =>
        list?.AsArray().Select(entry => entry!.GetValue<string>()).ToArray();

    public static Type? ExtractType(JsonNode? requested) => requested is null
        ? null
        : Types.GetValueOrDefault(requested.GetValue<string>()) ?? throw new InexpressibleInputException(requested.ToJsonString());

    public static bool? Strict(JsonNode? strict) => strict switch
    {
        null => null,
        JsonValue value when value.GetValueKind() is JsonValueKind.True or JsonValueKind.False => value.GetValue<bool>(),
        _ => throw new InexpressibleInputException(strict.ToJsonString()),
    };

    public static JsonObject? Options(JsonArray arguments, IReadOnlySet<string> keys)
    {
        var options = arguments.Count > 2 ? arguments[2] : null;
        if (options is null || SpecialValueDecoder.IsUnset(options))
        {
            return null;
        }
        if (options is not JsonObject members || members.Any(member => !keys.Contains(member.Key)))
        {
            throw new InexpressibleInputException(options.ToJsonString());
        }
        return members;
    }
}
