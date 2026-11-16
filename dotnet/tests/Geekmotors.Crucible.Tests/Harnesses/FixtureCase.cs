using System.Text.Json.Nodes;

namespace CrucibleAI.Tests.Harnesses;

internal sealed record FixtureCase(string Id, JsonNode? Input, JsonNode? Expect);
