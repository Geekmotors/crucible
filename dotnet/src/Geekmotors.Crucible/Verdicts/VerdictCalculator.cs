namespace CrucibleAI.Verdicts;

internal sealed class VerdictCalculator
{
    private const double NeedEpsilon = 1e-9;
    private const int MinimumNeed = 1;

    public Verdict Compute(double threshold, IReadOnlyCollection<RunStatus> statuses)
    {
        var need = Need(threshold, statuses.Count);
        var passed = statuses.Count(status => status == RunStatus.Passed);
        var failed = statuses.Count(status => status == RunStatus.Failed);
        var errored = statuses.Count(status => status == RunStatus.Errored);
        return new Verdict(Outcome(need, passed, errored), need, passed, failed, errored);
    }

    private static int Need(double threshold, int runs)
    {
        double product = threshold * runs;
        double lowered = product - NeedEpsilon;
        return Math.Max(MinimumNeed, (int)Math.Ceiling(lowered));
    }

    private static VerdictZone Outcome(int need, int passed, int errored)
    {
        if (passed >= need)
        {
            return VerdictZone.Passed;
        }
        return passed + errored < need ? VerdictZone.Failed : VerdictZone.Errored;
    }
}
