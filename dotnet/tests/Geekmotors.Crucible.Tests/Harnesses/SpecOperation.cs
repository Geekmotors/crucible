using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal sealed record SpecOperation(Func<JsonNode?, Task<JsonNode?>> Run, string OutcomeKey)
{
    private const string ValueKey = "value";
    private const string TextKey = "text";

    public static SpecOperation Value(Func<JsonNode?, JsonNode?> run) => new(input => Task.FromResult(run(input)), ValueKey);

    public static SpecOperation Value(Func<JsonNode?, Task<JsonNode?>> run) => new(run, ValueKey);

    public static SpecOperation Text(Func<JsonNode?, Task<JsonNode?>> run) => new(run, TextKey);

    public static SpecOperation Text(Func<JsonNode?, JsonNode?> run) => new(input => Task.FromResult(run(input)), TextKey);
}
