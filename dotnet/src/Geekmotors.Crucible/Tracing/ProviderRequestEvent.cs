using System.Text.Json.Nodes;

namespace CrucibleAI.Tracing;

internal sealed record ProviderRequestEvent(string Provider, string Model, string? System, JsonObject? Meta, string Prompt, int Attempt) : TraceEvent;
