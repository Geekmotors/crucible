using System.Text.Json;
using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal sealed record FixtureFile(string Name, string Text, string Area, string Operation, IReadOnlyList<FixtureCase> Cases)
{
    public string OperationKey => $"{Area}/{Operation}";

    public static FixtureFile Parse(string name, string text)
    {
        var root = JsonNode.Parse(text) as JsonObject ?? [];
        var cases = (root["cases"] as JsonArray ?? [])
            .Select(item => item as JsonObject ?? [])
            .Select(item => new FixtureCase(Member(item, "id"), item["input"], item["expect"]))
            .ToList();
        return new FixtureFile(name, text, Member(root, "area"), Member(root, "operation"), cases);
    }

    private static string Member(JsonObject members, string key) =>
        members[key] is JsonValue value && value.GetValueKind() == JsonValueKind.String ? value.GetValue<string>() : string.Empty;
}
