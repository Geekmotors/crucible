using System.Text.Json.Nodes;
using Geekmotors.Crucible.Urls;

namespace Geekmotors.Crucible.Config;

internal sealed record JudgeConfig(
    ProviderKind Provider,
    string Model,
    JsonObject? Meta,
    Verbosity Verbosity,
    RunOptions TestDefaults,
    int ExcerptChars,
    HttpUrl? BaseUrl,
    string? ApiKeyEnv);
