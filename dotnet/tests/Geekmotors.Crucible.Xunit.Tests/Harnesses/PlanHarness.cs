using CrucibleAI.Options;

namespace CrucibleAI.Xunit.Tests.Harnesses;

internal sealed class PlanHarness
{
    private readonly CrucibleSettingsResolver _plans = new();
    private readonly RunOptionsResolver _options = new();

    public ResolvedRunOptions Effective(Type testClass, ICrucibleSettings? method = null)
    {
        var plan = _plans.Resolve(testClass, method);
        return _options.Resolve([.. plan.Groups, plan.Declared]);
    }

    public bool Sequential(Type testClass, ICrucibleSettings? method = null) => _plans.Resolve(testClass, method).Sequential;

    public CrucibleException Rejection(Type testClass, ICrucibleSettings? method = null) =>
        Assert.Throws<CrucibleException>(() => _plans.Resolve(testClass, method));
}
