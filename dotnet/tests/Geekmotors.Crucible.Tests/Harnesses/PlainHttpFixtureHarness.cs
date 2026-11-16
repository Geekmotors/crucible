using System.Text.Json.Nodes;
using CrucibleAI.Providers;
using CrucibleAI.Urls;

namespace CrucibleAI.Tests.Harnesses;

internal static class PlainHttpFixtureHarness
{
    private static readonly HttpUrlParser Urls = new();
    private static readonly PlainHttpRule Rule = new();

    public static JsonNode? Decide(JsonNode? input) =>
        Rule.Warns(Urls.Parse(input!["baseUrl"]!.GetValue<string>())!, input["keySent"]!.GetValue<bool>());
}
