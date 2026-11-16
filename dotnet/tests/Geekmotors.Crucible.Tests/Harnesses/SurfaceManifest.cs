using System.Text.Json.Nodes;

namespace CrucibleAI.Tests.Harnesses;

internal static class SurfaceManifest
{
    private const string Resource = "spec/surface.json";

    private static readonly string[] NamedSections =
        ["operations", "functions", "runOptions", "configFields", "providers", "environment", "verdictOutcomes", "reportText"];

    private static readonly Lazy<IReadOnlyList<SurfaceItem>> Loaded = new(Load);

    public static IReadOnlyList<SurfaceItem> Items => Loaded.Value;

    public static TheoryData<string> Keys => new(Items.Select(item => item.Key));

    public static SurfaceItem Item(string key) => Items.Single(item => item.Key == key);

    private static List<SurfaceItem> Load()
    {
        var manifest = JsonNode.Parse(SpecFixtureCatalog.Read(Resource))!.AsObject();
        var items = new List<SurfaceItem>();
        foreach (var section in NamedSections)
        {
            items.AddRange(manifest[section]!.AsArray().Select(entry => Item($"{section}:{Text(entry, "name")}", entry)));
        }
        items.AddRange(manifest["callOptions"]!.AsArray()
            .Select(entry => Item($"callOptions:{Text(entry, "assertion")}.{Text(entry, "name")}", entry)));
        items.AddRange(manifest["errorKinds"]!.AsArray()
            .Select(entry => Item($"errorKinds:{Text(entry, "kind")}{Retryable(entry)}", entry)));
        return items;
    }

    private static SurfaceItem Item(string key, JsonNode? entry) =>
        new(key, entry?["fixtures"]?.AsArray().Select(id => id!.GetValue<string>()).ToArray() ?? []);

    private static string Retryable(JsonNode? entry) =>
        entry?["retryable"] is { } retryable ? $"(retryable: {(retryable.GetValue<bool>() ? "true" : "false")})" : string.Empty;

    private static string Text(JsonNode? entry, string member) => entry![member]!.GetValue<string>();
}
