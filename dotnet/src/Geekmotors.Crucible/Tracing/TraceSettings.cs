using System.Text.Json.Nodes;
using Geekmotors.Crucible.Config;

namespace Geekmotors.Crucible.Tracing;

internal sealed record TraceSettings(Verbosity Verbosity, string Provider, string Model, JsonObject? Meta, JsonObject TestDefaults, int ExcerptChars);
