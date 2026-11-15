using System.Text.Json.Nodes;
using Geekmotors.Crucible.Options;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class RunOptionsJson
{
    public static JsonObject Of(RunOptions options)
    {
        var json = new JsonObject();
        if (options.Runs is { } runs)
        {
            json["runs"] = runs;
        }
        if (options.Threshold is { } threshold)
        {
            json["threshold"] = threshold;
        }
        if (options.InferenceTimeout is { } inferenceTimeout)
        {
            json["inferenceTimeout"] = inferenceTimeout;
        }
        if (options.TestTimeout is { } testTimeout)
        {
            json["testTimeout"] = testTimeout;
        }
        if (options.Strict is { } strict)
        {
            json["strict"] = strict;
        }
        return json;
    }

    public static JsonObject Of(ResolvedRunOptions options) => new()
    {
        ["runs"] = options.Runs,
        ["threshold"] = options.Threshold,
        ["inferenceTimeout"] = options.InferenceTimeout,
        ["testTimeout"] = options.TestTimeout,
        ["strict"] = options.Strict,
    };
}
