namespace CrucibleAI.Options;

internal sealed class RunOptionsResolver
{
    private const int DefaultRuns = 10;
    private const double DefaultThreshold = 0.9;
    private const int DefaultInferenceTimeout = 30000;
    private const bool DefaultStrict = false;
    private const long TestTimeoutBase = 30000;
    private const long TestTimeoutPerExtraRun = 1000;

    public ResolvedRunOptions Resolve(IEnumerable<RunOptions> layersOuterToInner)
    {
        var declared = layersOuterToInner.Aggregate(new RunOptions(), Nearer);
        var runs = declared.Runs ?? DefaultRuns;
        return new ResolvedRunOptions(
            runs,
            declared.Threshold ?? DefaultThreshold,
            declared.InferenceTimeout ?? DefaultInferenceTimeout,
            declared.TestTimeout ?? TestTimeoutFor(runs),
            declared.Strict ?? DefaultStrict);
    }

    private static RunOptions Nearer(RunOptions outer, RunOptions inner) => new()
    {
        Runs = inner.Runs ?? outer.Runs,
        Threshold = inner.Threshold ?? outer.Threshold,
        InferenceTimeout = inner.InferenceTimeout ?? outer.InferenceTimeout,
        TestTimeout = inner.TestTimeout ?? outer.TestTimeout,
        Strict = inner.Strict ?? outer.Strict,
    };

    private static int TestTimeoutFor(int runs) =>
        (int)Math.Min(int.MaxValue, TestTimeoutBase + ((runs - 1L) * TestTimeoutPerExtraRun));
}
