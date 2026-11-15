using CrucibleAI.Xunit.Tests.Constants;

namespace CrucibleAI.Xunit.Tests.Harnesses;

public sealed class SingleThreadFixtureRunHarness : FixtureRunHarness
{
    protected override IEnumerable<string> Options => ["-maxThreads", "1", "-namespace", FixtureTests.LifecycleNamespace];
}
