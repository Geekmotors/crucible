using System.Text.Json.Nodes;
using CrucibleAI.Config;

namespace CrucibleAI.Tracing;

internal sealed record TraceSettings(Verbosity Verbosity, string Provider, string Model, JsonObject? Meta, JsonObject TestDefaults, int ExcerptChars);
