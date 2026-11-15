using System.Text.Json.Nodes;
using Geekmotors.Crucible.Json;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class JsoncFixtureHarness
{
    private static readonly JsoncReader Reader = new();

    public static JsonNode? Parse(JsonNode? input) => Reader.Read(input!["text"]!.GetValue<string>());
}
