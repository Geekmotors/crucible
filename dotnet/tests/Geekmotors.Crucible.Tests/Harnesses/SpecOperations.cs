namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class SpecOperations
{
    public const string RunOptionsResolve = "run-options/resolve";
    public const string RunOptionsValidate = "run-options/validate";
    public const string VerdictCompute = "verdict/compute";
    public const string JsoncParse = "jsonc/parse";
    public const string ConfigLoad = "config/load";
    public const string ProviderRequest = "provider/request";
    public const string ProviderResponse = "provider/response";
    public const string ProviderPlainHttpWarning = "provider/plain-http-warning";
    public const string JudgeReply = "judge/reply";

    private const string UnsetUnknownKeyReason =
        "Neither C# input can hold an unknown key whose value is unset: RunOptions has no member for an unknown key, "
        + "and JSON has no unset value (null is a value, rejected by the *-null cases). "
        + "run-options-validate-*-unknown-key covers the rule; the reader rejects an unknown key whatever its value.";

    public static IReadOnlyDictionary<string, SpecOperation> Implemented { get; } =
        new Dictionary<string, SpecOperation>(StringComparer.Ordinal)
        {
            [RunOptionsResolve] = SpecOperation.Value(RunOptionsFixtureHarness.Resolve),
            [RunOptionsValidate] = SpecOperation.Value(RunOptionsFixtureHarness.Validate),
            [VerdictCompute] = SpecOperation.Value(VerdictFixtureHarness.Compute),
            [JsoncParse] = SpecOperation.Value(JsoncFixtureHarness.Parse),
            [ConfigLoad] = SpecOperation.Value(ConfigFixtureHarness.Load),
            [ProviderRequest] = SpecOperation.Value(ProviderFixtureHarness.RequestAsync),
            [ProviderResponse] = SpecOperation.Text(ProviderFixtureHarness.ResponseAsync),
            [ProviderPlainHttpWarning] = SpecOperation.Value(PlainHttpFixtureHarness.Decide),
            [JudgeReply] = SpecOperation.Value(JudgeReplyFixtureHarness.AskAsync),
        };

    public static IReadOnlySet<string> Pending { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
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
