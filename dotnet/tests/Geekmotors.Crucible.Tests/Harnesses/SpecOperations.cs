using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class SpecOperations
{
    public const string RunOptionsResolve = "run-options/resolve";
    public const string RunOptionsValidate = "run-options/validate";
    public const string VerdictCompute = "verdict/compute";

    private const string UnsetUnknownKeyReason =
        "Neither C# input can hold an unknown key whose value is unset: RunOptions has no member for an unknown key, "
        + "and JSON has no unset value (null is a value, rejected by the *-null cases). "
        + "run-options-validate-*-unknown-key covers the rule; the reader rejects an unknown key whatever its value.";

    public static IReadOnlyDictionary<string, Func<JsonNode?, JsonNode?>> Implemented { get; } =
        new Dictionary<string, Func<JsonNode?, JsonNode?>>(StringComparer.Ordinal)
        {
            [RunOptionsResolve] = RunOptionsFixtureHarness.Resolve,
            [RunOptionsValidate] = RunOptionsFixtureHarness.Validate,
            [VerdictCompute] = VerdictFixtureHarness.Compute,
        };

    public static IReadOnlySet<string> Pending { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "jsonc/parse",
        "config/load",
        "provider/request",
        "provider/response",
        "provider/plain-http-warning",
        "judge/reply",
        "payload/coherent",
        "payload/extract",
        "report/render",
        "report/trace",
        "assertion/call",
    };

    public static IReadOnlyDictionary<string, string> Skipped { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["run-options-validate-test-unknown-key-unset"] = UnsetUnknownKeyReason,
        ["run-options-validate-group-unknown-key-unset"] = UnsetUnknownKeyReason,
    };

    public static IEnumerable<string> Covered => Implemented.Keys.Concat(Pending);
}
