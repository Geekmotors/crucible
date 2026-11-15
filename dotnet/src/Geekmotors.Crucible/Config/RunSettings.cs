using System.Text.Json.Nodes;

namespace CrucibleAI.Config;

internal sealed record RunSettings(Verbosity Verbosity, RunOptions TestDefaults, JsonObject TestDefaultsNode, int ExcerptChars);
