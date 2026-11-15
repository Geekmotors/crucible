using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal sealed record FixtureCase(string Id, JsonNode? Input, JsonNode? Expect);
