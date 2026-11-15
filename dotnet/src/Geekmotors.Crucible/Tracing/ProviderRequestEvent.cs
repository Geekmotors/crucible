using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Tracing;

internal sealed record ProviderRequestEvent(string Provider, string Model, string? System, JsonObject? Meta, string Prompt, int Attempt) : TraceEvent;
